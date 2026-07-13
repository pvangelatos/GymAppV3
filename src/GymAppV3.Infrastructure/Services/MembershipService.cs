using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs.Membership;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Infrastructure.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeProvider _clock;

        public MembershipService(ApplicationDbContext context, IDateTimeProvider clock)
        {
            _context = context;
            _clock = clock;
        }
        public async Task<MembershipDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Memberships
                .Where(m => m.Id == id)
                .Select(m => new MembershipDto(
                    m.Id, m.MemberId, m.MembershipPackageId, m.MembershipPackage.Name,
                    m.PricePaid, m.StartDate, m.EndDate, m.RemainingSessions,
                     m.Status.ToString()))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<MembershipDto>> GetByMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
        {
            var memberships = await _context.Memberships
                .Where(m => m.MemberId == memberId)
                .Select(m => new MembershipDto(
                    m.Id, m.MemberId, m.MembershipPackageId, m.MembershipPackage.Name,
                    m.PricePaid, m.StartDate, m.EndDate, m.RemainingSessions,
                     m.Status.ToString()))
                .ToListAsync(cancellationToken);

            // Order in memory (DateTimeOffset ordering is unreliable in SQLite).
            return memberships.OrderByDescending(m => m.StartDate).ToList();
        }

        public async Task<MembershipDto> PurchaseAsync(PurchaseMembershipRequest request, CancellationToken cancellationToken = default)
        {
            // --- The member must exist (soft-deleted ones are already filtered out) ---
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken) ??
                throw new NotFoundException(nameof(Member), request.MemberId);

            // --- The package must exist ---
            var package = await _context.MembershipPackages
                .FirstOrDefaultAsync(p => p.Id == request.MembershipPackageId, cancellationToken) ??
                throw new NotFoundException(nameof(MembershipPackage), request.MembershipPackageId);

            var now = _clock.UtcNow;

            // --- Renewal stacking: find the latest end date among the member's existing
            // memberships for the SAME package. A new purchase of the same package starts
            // when the previous one ends; a first purchase (or a different package) starts now.
            // DateTimeOffset comparison is done in memory (SQLite limitation).
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

            // --- Build the membership. Price, sessions and status are derived from the
            // package — never taken from client input. PricePaid is a snapshot of the
            // package's price at purchase time. ---
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

            return MapToDto(membership, package.Name);
        }

        private static MembershipDto MapToDto(Membership m, string packageName) =>
        new(
            m.Id, m.MemberId, m.MembershipPackageId, packageName,
            m.PricePaid, m.StartDate, m.EndDate, m.RemainingSessions, m.Status.ToString());
    }
}
