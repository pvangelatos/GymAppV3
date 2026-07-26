using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Memberships;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.Member.Memberships;

/// <summary>
/// Handlers for member-scoped membership endpoints.
/// </summary>
public static class MembershipHandlers
{
    public static async Task<Ok<IReadOnlyList<MembershipDto>>> GetByMemberAsync(
        Guid memberId,
        IMembershipQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetByMemberAsync(new GetMembershipsByMemberQuery(memberId), cancellationToken);
        return TypedResults.Ok(result);
    }
}
