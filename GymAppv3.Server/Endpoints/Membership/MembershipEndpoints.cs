using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints.Membership;

public static class MembershipEndpoints
{
    public static IEndpointRouteBuilder MapMembershipEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/memberships")
            .WithTags("Memberships");

        var memberMemberships = app.MapGroup("/api/members/{memberId:guid}/memberships")
            .WithTags("Memberships");

        group.MapPost("/", MembershipHandlers.PurchaseAsync)
            .WithName("PurchaseMembership")
            .RequireAuthorization()
            .Accepts<PurchaseMembershipCommand>("application/json")
            .Produces<MembershipDto>(StatusCodes.Status201Created);

        group.MapGet("/{id:guid}", MembershipHandlers.GetByIdAsync)
            .WithName("GetMembershipById")
            .RequireAuthorization()
            .Produces<MembershipDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        memberMemberships.MapGet("/", MembershipHandlers.GetByMemberAsync)
            .WithName("GetMembershipsByMember")
            .RequireAuthorization()
            .Produces<IReadOnlyList<MembershipDto>>(StatusCodes.Status200OK);

        return app;
    }
}
