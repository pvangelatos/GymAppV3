using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.Members;

namespace GymAppV3.Core.Interfaces;

/// <summary>
/// Service interface for member query operations.
/// Returns authorization-aware DTOs based on the requesting user's role.
/// </summary>
public interface IMemberQueryService
{
    /// <summary>
    /// Gets a member by ID.
    /// Returns MemberDetailDto if user has authorization to see medical notes, otherwise MemberDto.
    /// </summary>
    Task<MemberDetailDto?> GetByIdAsync(GetMemberByIdQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the member profile bound to a user account. Used by GET /api/members/me.
    /// Returns null when the user has registered but not yet completed step 2.
    /// </summary>
    Task<MemberDto?> GetByUserIdAsync(GetMemberByUserIdQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all members with optional filtering and pagination.
    /// </summary>
    Task<ResultSet<MemberDto>> GetAllAsync(GetAllMembersQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets members who have active bookings (paginated).
    /// </summary>
    Task<ResultSet<MemberDto>> GetByActiveBookingsAsync(GetMembersByActiveBookingsQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets members who have active memberships (paginated).
    /// </summary>
    Task<ResultSet<MemberDto>> GetByActiveMembershipAsync(GetMembersByActiveMembershipQuery query, CancellationToken cancellationToken = default);
}
