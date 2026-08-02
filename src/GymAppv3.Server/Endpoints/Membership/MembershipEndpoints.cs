using GymAppv3.Server.Endpoints.Common;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.Membership;

/// <summary>
/// Top-level membership endpoints (admin operations for all memberships).
/// Member-scoped membership routes are in Member/Memberships/.
/// </summary>
public static class MembershipEndpoints
{
    public static IEndpointRouteBuilder MapMembershipEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/memberships")
            .WithTags("Memberships");

        var memberMemberships = app.MapGroup("/api/members/{memberId:guid}/memberships")
            .WithTags("Memberships")
            .AddEndpointFilter<MemberScopedFilter>();

        group.MapPost("/", MembershipHandlers.PurchaseAsync)
            .WithName("PurchaseMembership")
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<PurchaseMembershipCommand>>()
            .Accepts<PurchaseMembershipCommand>("application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<MembershipDto>(StatusCodes.Status201Created);

        group.MapGet("/{id:guid}", MembershipHandlers.GetByIdAsync)
            .WithName("GetMembershipById")
            .RequireAuthorization()
            .Produces<MembershipDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
