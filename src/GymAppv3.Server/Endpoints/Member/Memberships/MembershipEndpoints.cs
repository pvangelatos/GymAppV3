using GymAppv3.Server.Endpoints.Common;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints.Member.Memberships;

/// <summary>
/// Member-scoped membership endpoints.
/// Top-level membership operations are in Endpoints/Membership/.
/// </summary>
public static class MembershipEndpoints
{
    public static IEndpointRouteBuilder MapMemberMembershipEndpoints(this IEndpointRouteBuilder app)
    {
        var memberMemberships = app.MapGroup("/api/members/{memberId:guid}/memberships")
            .WithTags("Memberships");

        memberMemberships.MapGet("/", MembershipHandlers.GetByMemberAsync)
            .WithName("GetMembershipsByMember")
            .RequireAuthorization()
            .Produces<IReadOnlyList<MembershipDto>>(StatusCodes.Status200OK);

        return app;
    }
}
