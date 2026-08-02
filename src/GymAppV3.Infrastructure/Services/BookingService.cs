using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Core.Queries.Bookings;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using GymAppV3.Core.Common;
using GymAppV3.Infrastructure.Identity;

namespace GymAppV3.Infrastructure.Services;

public class BookingService : IBookingCommandService, IBookingQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;
    private readonly IUserContext _userContext;

    public BookingService(ApplicationDbContext context, IDateTimeProvider clock, IUserContext userContext)
    {
        _context = context;
        _clock = clock;
        _userContext = userContext;
    }

    public async Task<BookingDto> BookAsync(CreateBookingCommand request, CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;

        // --- Member and Session existence checks ---
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.MemberId);

        _userContext.EnsureCanActOnBehalfOfMember(member);

        var session = await _context.ClassSessions
            .FirstOrDefaultAsync(s => s.Id == request.ClassSessionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ClassSession), request.ClassSessionId);

        // --- Business Rule: Session timing ---
        if (session.StartsAt <= now)
            throw new BusinessRuleException("Cannot book a session that has already started.");

        // --- Business Rule: Available seats check ---
        if (session.AvailableSeats <= 0)
            throw new BusinessRuleException("The session is fully booked.");

        // --- Business Rule: Prevent duplicate active booking ---
        var alreadyBooked = await _context.Bookings
            .AnyAsync(b => b.ClassSessionId == session.Id
                        && b.MemberId == member.Id
                        && b.Status == BookingStatus.Confirmed,
                      cancellationToken);
        if (alreadyBooked)
            throw new BusinessRuleException("You already have a booking for this session.");

        // --- Find active membership with remaining balance for this class category ---
        // Pick the one expiring soonest so expiring credits are consumed first.
        var membership = await _context.Memberships
            .Where(m => m.MemberId == member.Id
                     && m.Status == MembershipStatus.Active
                     && m.RemainingSessions > 0
                     && m.MembershipPackage.ClassCategoryId == session.ClassCategoryId
                     && m.StartDate <= now
                     && m.EndDate >= now)
            .OrderBy(m => m.EndDate)
            .FirstOrDefaultAsync(cancellationToken) ??
            throw new BusinessRuleException(
                "No active membership with remaining sessions covers this class category.");

        // --- Execute mutations ---
        var booking = new Booking
        {
            MemberId = member.Id,
            Member = member,
            ClassSessionId = session.Id,
            ClassSession = session,
            Status = BookingStatus.Confirmed,
            BookedAt = now
        };

        session.AvailableSeats--;        // one fewer seat
        membership.RemainingSessions--;  // one fewer credit

        _context.Bookings.Add(booking);

        // SaveChanges executes atomically. Optimistic Concurrency (RowVersion) prevents race conditions.
        await _context.SaveChangesAsync(cancellationToken);

        // Fetch clean DTO via ObjectMapper projection
        return await _context.Bookings
            .Where(b => b.Id == booking.Id)
            .Select(ObjectMapper.Booking.ToDto)
            .FirstAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;

        var booking = await _context.Bookings
            .Include(b => b.ClassSession)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken)
            ?? throw new NotFoundException(nameof(Booking), bookingId);

        _userContext.EnsureCanActOnBehalfOfMember(booking.Member);

        // Only confirmed bookings can be cancelled.
        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only a confirmed booking can be cancelled.");

        var session = booking.ClassSession;

        // Mark the booking cancelled either way.
        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = now;

        // The seat is always freed — the spot becomes available again.
        session.AvailableSeats++;

        // --- 24-Hour Policy Rule ---
        // Session credit is refunded ONLY if cancelled >= 24h ahead of class start time.
        var hoursUntilStart = (session.StartsAt - now).TotalHours;
        if (hoursUntilStart >= 24)
        {
            // Find the membership this booking was charged from: same member, same
            // category as the session. Return one credit to it.
            var membership = await FindMembershipToRefund(
                booking.MemberId, session.ClassCategoryId, cancellationToken);

            if (membership is not null)
                membership.RemainingSessions++;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ResultSet<BookingDto>> GetByMemberAsync(GetBookingsByMemberQuery query, CancellationToken cancellationToken)
    {
        if (!_userContext.IsStaff())
        {
            var userId = _userContext.RequireUserId();
            var isOwner = await _context.Members.AnyAsync(m => m.Id == query.MemberId && m.UserId == userId, cancellationToken);
            if (!isOwner)
                throw new ForbiddenException("You are not allowed to view this member's bookings.");
        }
        // Base query: all bookings for the member
        var q = _context.Bookings.Where(b => b.MemberId == query.MemberId);

        // If OnlyActive is true, filter to only confirmed bookings for future sessions
        if (query.OnlyActive)
        {   
            var now = _clock.UtcNow;
            q = q.Where(b => b.Status == BookingStatus.Confirmed
                          && b.ClassSession.StartsAt > now);
        }

        // Order the results based on the OnlyActive flag
        var ordered = !string.IsNullOrWhiteSpace(query.Options?.Sort)
            ? q.ApplySorting(query.Options.Sort)
            : query.OnlyActive
            ? q.OrderBy(b => b.ClassSession.StartsAt).ThenBy(b => b.Id)
            : q.OrderByDescending(b => b.BookedAt).ThenBy(b => b.Id);

        // Project to DTOs and return a paginated result set
        return await ordered
            .Select(ObjectMapper.Booking.ToDto)
            .ToResultSetAsync(query.Options?.Page ?? 1, query.Options?.Size ?? 50, cancellationToken);
    }


    // Finds an active membership of the given category to refund a session credit to.
    // Prefers the one ending soonest, mirroring how BookAsync chooses which to spend.
    private async Task<Membership?> FindMembershipToRefund(
        Guid memberId, Guid categoryId, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        return await _context.Memberships
        .Where(m => m.MemberId == memberId
                 && m.Status == MembershipStatus.Active
                 && m.MembershipPackage.ClassCategoryId == categoryId
                 && m.StartDate <= now
                 && m.EndDate >= now)
        .OrderBy(m => m.EndDate)
        .FirstOrDefaultAsync(cancellationToken);
    }
}
