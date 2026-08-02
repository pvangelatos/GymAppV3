using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Core.Queries.Memberships;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Services;

public class MembershipService : IMembershipCommandService, IMembershipQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;
    private readonly IPaymentCommandService _paymentCommandService;
    private readonly IUserContext _userContext;


    public MembershipService(ApplicationDbContext context,
        IDateTimeProvider clock,
        IPaymentCommandService paymentCommandService,
        IUserContext userContext)
    {
        _context = context;
        _clock = clock;
        _paymentCommandService = paymentCommandService;
        _userContext = userContext;
    }
    public async Task<MembershipDto?> GetByIdAsync(GetMembershipByIdQuery query, CancellationToken cancellationToken = default)
    {
        if (!_userContext.IsStaff())
        {
            var userId = _userContext.RequireUserId();
            var isOwner = await _context.Memberships
                .AnyAsync(m => m.Id == query.Id && m.Member.UserId == userId, cancellationToken);
            if (!isOwner)
                throw new ForbiddenException("You are not allowed to view this membership.");
        }

        return await _context.Memberships
            .Where(m => m.Id == query.Id)
            .Select(ObjectMapper.Membership.ToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MembershipDto>> GetByMemberAsync(GetMembershipsByMemberQuery query, CancellationToken cancellationToken = default)
    {
        if (!_userContext.IsStaff())
        {
            var userId = _userContext.RequireUserId();
            var isOwner = await _context.Members.AnyAsync(m => m.Id == query.MemberId && m.UserId == userId, cancellationToken);
            if (!isOwner)
                throw new ForbiddenException("You are not allowed to view this member's memberships.");
        }

        // Existence validation for the member
        var q = _context.Memberships
            .Where(m => m.MemberId == query.MemberId);

        // Filter for only active memberships if requested
        if (query.OnlyActive)
        {
            var now = _clock.UtcNow;
            q = q.Where(m => m.Status == MembershipStatus.Active && m.EndDate > now);
        }

        // Order by StartDate descending and project to DTO
        return await q
            .OrderByDescending(m => m.StartDate)
            .Select(ObjectMapper.Membership.ToDto)
            .ToListAsync(cancellationToken);

    }

    public async Task<MembershipDto> PurchaseAsync(PurchaseMembershipCommand request, CancellationToken cancellationToken = default)
    {
        // --- Member existence validation ---
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken) ??
            throw new NotFoundException(nameof(Member), request.MemberId);

        _userContext.EnsureCanActOnBehalfOfMember(member);

        // --- Package existence validation ---
        var package = await _context.MembershipPackages
            .FirstOrDefaultAsync(p => p.Id == request.MembershipPackageId, cancellationToken) ??
            throw new NotFoundException(nameof(MembershipPackage), request.MembershipPackageId);

        var now = _clock.UtcNow;

        // --- Renewal Stacking ---
        // Latest end-date of any active, non-expired membership for the same package.
        // Cast to nullable so MaxAsync returns null on empty set instead of throwing.
        var latestEnd = await _context.Memberships
            .Where(m => m.MemberId == request.MemberId
                     && m.MembershipPackageId == request.MembershipPackageId
                     && m.Status == MembershipStatus.Active
                     && m.EndDate > now)
            .Select(m => (DateTimeOffset?)m.EndDate)
            .MaxAsync(cancellationToken);

        var startDate = latestEnd ?? now;
        var endDate = startDate.AddDays(package.DurationInDays);

        // --- Price snapshot ---
        // PricePaid freezes the package price at the exact moment of purchase.
        var membership = new Membership
        {
            MemberId = member.Id,
            MembershipPackageId = package.Id,
            PricePaid = package.Price,                     // frozen snapshot
            StartDate = startDate,
            EndDate = endDate,
            RemainingSessions = package.SessionsIncluded,
            Status = MembershipStatus.Active
        };

        // --- Atomic: membership + payment succeed or fail together ---
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync(cancellationToken);

        await _paymentCommandService.RecordAsync(
            new RecordPaymentCommand(member.Id, membership.Id, package.Price, request.Method),
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return ObjectMapper.Membership.ToDtoCompiled(membership);
    }

}
