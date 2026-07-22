using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Memberships;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.Membership;

public static class MembershipHandlers
{
    public static async Task<Results<Ok<MembershipDto>, NotFound>> GetByIdAsync(
        Guid id,
        IMembershipQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetByIdAsync(new GetMembershipByIdQuery(id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Ok<IReadOnlyList<MembershipDto>>> GetByMemberAsync(
        Guid memberId,
        IMembershipQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetByMemberAsync(new GetMembershipsByMemberQuery(memberId), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Created<MembershipDto>> PurchaseAsync(
        PurchaseMembershipCommand command,
        IMembershipCommandService commandService,
        CancellationToken cancellationToken)
    {
        var created = await commandService.PurchaseAsync(command, cancellationToken);
        return TypedResults.Created($"/api/memberships/{created.Id}", created);
    }
}
