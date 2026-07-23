using GymAppV3.Core.Common;

namespace GymAppV3.Core.Queries.Members;

/// <summary>
/// Query to get members who have active memberships.
/// Supports pagination.
/// </summary>
public record GetMembersByActiveMembershipQuery(ListOptions? Options = null);
