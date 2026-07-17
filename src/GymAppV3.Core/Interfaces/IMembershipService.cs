using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Interfaces
{
    // Use-case service for purchasing and reading memberships. PurchaseAsync derives
    // dates, price snapshot, and session balance from the package, and stacks renewals
    // of the same package onto the end of the member's existing one.
    public interface IMembershipService
    {
        // Purchases a membership for a member. If the member already has an active or
        // future membership for the SAME package, the new one starts when that ends
        // (renewal stacking). Different packages run in parallel from now.
        Task<MembershipDto> PurchaseAsync(
            PurchaseMembershipCommand request, CancellationToken cancellationToken = default);

        // Returns a single membership by id, or null if not found.
        Task<MembershipDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // Returns all memberships belonging to a member, newest start date first.
        Task<IReadOnlyList<MembershipDto>> GetByMemberAsync(
            Guid memberId, CancellationToken cancellationToken = default);
    }
}
