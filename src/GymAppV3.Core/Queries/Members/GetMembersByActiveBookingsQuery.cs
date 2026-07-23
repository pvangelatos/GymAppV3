using GymAppV3.Core.Common;

namespace GymAppV3.Core.Queries.Members;

/// <summary>
/// Query to get members who have active bookings.
/// Supports pagination.
/// </summary>
public record GetMembersByActiveBookingsQuery(ListOptions? Options = null);
