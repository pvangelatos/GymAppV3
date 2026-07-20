using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Core.Queries.Memberships;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Services
{
    public class MembershipService : IMembershipCommandService, IMembershipQueryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeProvider _clock;

        public MembershipService(ApplicationDbContext context, IDateTimeProvider clock)
        {
            _context = context;
            _clock = clock;
        }
        public async Task<MembershipDto?> GetByIdAsync(GetMembershipByIdQuery query, CancellationToken cancellationToken = default)
        {
            return await _context.Memberships
                .Where(m => m.Id == query.Id)
                .Select(m => new MembershipDto(
                    m.Id, m.MemberId, m.MembershipPackageId, m.MembershipPackage.Name,
                    m.PricePaid, m.StartDate, m.EndDate, m.RemainingSessions,
                     m.Status.ToString()))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<MembershipDto>> GetByMemberAsync(GetMembershipsByMemberQuery query, CancellationToken cancellationToken = default)
        {
            var memberships = await _context.Memberships
                .Where(m => m.MemberId == query.MemberId)
                .Select(ObjectMapper.Membership.ToDto)
                .ToListAsync(cancellationToken);

            return memberships.OrderByDescending(m => m.StartDate).ToList();
        }

        public async Task<MembershipDto> PurchaseAsync(PurchaseMembershipCommand request, CancellationToken cancellationToken = default)
        {
            // --- Member existence validation ---
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken) ??
                throw new NotFoundException(nameof(Member), request.MemberId);

            // --- Package existence validation ---
            var package = await _context.MembershipPackages
                .FirstOrDefaultAsync(p => p.Id == request.MembershipPackageId, cancellationToken) ??
                throw new NotFoundException(nameof(MembershipPackage), request.MembershipPackageId);

            var now = _clock.UtcNow;

            // --- Renewal Stacking ---
            // If the member has an unexpired membership for the same package, the new package starts when the previous one ends.
            var samePackageMemberships = await _context.Memberships
                .Where(m => m.MemberId ==  request.MemberId
                         && m.MembershipPackageId == request.MembershipPackageId
                         && m.Status == MembershipStatus.Active)
                .Select(m => m.EndDate)
                .ToListAsync(cancellationToken);

            var latestEnd = samePackageMemberships
                .Where(end => end > now)                // only memberships that haven't expired yet
                .DefaultIfEmpty(now)                    // if none, fall back to "now"
                .Max();

            var startDate = latestEnd;
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
                Status = MembershipStatus.Active,
                RowVersion = Array.Empty<byte>()
            };

            _context.Memberships.Add(membership);
            await _context.SaveChangesAsync(cancellationToken);

            return ObjectMapper.Membership.ToDtoCompiled(membership);
        }

    }
}
