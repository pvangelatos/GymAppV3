using GymAppV3.Core.Common;

namespace GymAppV3.Core.Queries.Members;

/// <summary>
/// Query to get all members with optional filtering and pagination.
/// Supports filtering by name, email, and active membership status.
/// </summary>
public record GetAllMembersQuery(
    ListOptions? Options = null,
    string? SearchTerm = null,
    bool? HasActiveMembership = null);
