using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeProvider _clock;

        public BookingService(ApplicationDbContext context, IDateTimeProvider clock)
        {
            _context = context;
            _clock = clock;
        }

        public async Task<BookingDto> BookAsync(CreateBookingCommand request, CancellationToken cancellationToken = default)
        {
            var now = _clock.UtcNow;

            // --- The member must exist ---
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
                ?? throw new NotFoundException(nameof(Member), request.MemberId);

            // --- The session must exist ---
            var session = await _context.ClassSessions
                .FirstOrDefaultAsync(s => s.Id == request.ClassSessionId, cancellationToken)
                ?? throw new NotFoundException(nameof(ClassSession), request.ClassSessionId);

            // --- The session must not have started yet ---
            if (session.StartsAt <= now)
                throw new BusinessRuleException("Cannot book a session that has already started.");

            // --- The session must have a free seat ---
            if (session.AvailableSeats <= 0)
                throw new BusinessRuleException("The session is fully booked.");

            // --- The member must not already have an active booking for this session ---
            var alreadyBooked = await _context.Bookings
                .AnyAsync(b => b.ClassSessionId == session.Id
                            && b.MemberId == member.Id
                            && b.Status == BookingStatus.Confirmed,
                          cancellationToken);
            if (alreadyBooked)
                throw new BusinessRuleException("You already have a booking for this session.");

            // --- Find a membership that covers this session's category, with balance ---
            // DateTimeOffset comparisons are done in memory (SQLite limitation), so we
            // pull the candidate memberships first and pick in C#.
            var candidateMemberships = await _context.Memberships
                .Where(m => m.MemberId == member.Id
                         && m.Status == MembershipStatus.Active
                         && m.RemainingSessions > 0
                         && m.MembershipPackage.ClassCategoryId == session.ClassCategoryId)
                .ToListAsync(cancellationToken);

            // Only memberships whose validity window includes "now", and among those,
            // spend the one that ends soonest (use up expiring credit first).
            var membership = candidateMemberships
                .Where(m => m.StartDate <= now && m.EndDate >= now)
                .OrderBy(m => m.EndDate)
                .FirstOrDefault() ??
                throw new BusinessRuleException("No active membership with remaining sessions covers this class category.");

            // --- All rules passed. Apply the three changes atomically. ---
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

            // SaveChanges writes all three changes in a single transaction. The
            // RowVersion tokens on session and membership guard against concurrent
            // bookings both decrementing the same values.
            await _context.SaveChangesAsync(cancellationToken);

            return ObjectMapper.Booking.ToDtoCompiled(booking);
        }

        public async Task CancelAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var now = _clock.UtcNow;

            var booking = await _context.Bookings
                .Include(b => b.ClassSession)
                .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken)
                ?? throw new NotFoundException(nameof(Booking), bookingId);

            // Only confirmed bookings can be cancelled.
            if (booking.Status != BookingStatus.Confirmed)
                throw new BusinessRuleException("Only a confirmed booking can be cancelled.");

            var session = booking.ClassSession;

            // Mark the booking cancelled either way.
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = now;

            // The seat is always freed — the spot becomes available again.
            session.AvailableSeats++;

            // Session credit is returned ONLY if cancelling more than 24h ahead.
            // Within 24h the credit is forfeited (the member loses that session).
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

        public async Task<IReadOnlyList<BookingDto>> GetByMemberAsync(Guid memberId, CancellationToken cancellationToken = default) =>
                    await _context.Bookings
                            .Where(b => b.MemberId == memberId)
                            .Select(ObjectMapper.Booking.ToDto)
                            .ToListAsync(cancellationToken);

        // Finds an active membership of the given category to refund a session credit to.
        // Prefers the one ending soonest, mirroring how BookAsync chooses which to spend.
        private async Task<Membership?> FindMembershipToRefund(
            Guid memberId, Guid categoryId, CancellationToken cancellationToken)
        {
            var candidates = await _context.Memberships
                .Where(m => m.MemberId == memberId
                         && m.Status == MembershipStatus.Active
                         && m.MembershipPackage.ClassCategoryId == categoryId)
                .ToListAsync(cancellationToken);

            var now = _clock.UtcNow;
            return candidates
                .Where(m => m.StartDate <= now && m.EndDate >= now)
                .OrderBy(m => m.EndDate)
                .FirstOrDefault();
        }
    }
}
