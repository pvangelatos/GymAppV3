using GymAppv3.Server.Endpoints.Member;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.Members;

namespace GymAppv3.Server.Endpoints.Member;

/// <summary>
/// Extension methods for mapping member endpoints.
/// </summary>
public static class MemberEndpoints
{
    public static IEndpointRouteBuilder MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members")
            .WithTags("Members");

        // Literal segments first. The {id:guid} constraint would reject "me" anyway,
        // but keeping the specific routes above the generic one reads better.

        group.MapPost("/me", MemberHandlers.CompleteProfileAsync)
            .WithName("CompleteMemberProfile")
            .RequireAuthorization()
            .Accepts<CompleteMemberProfileCommand>("application/json")
            .Produces<MemberDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/me", MemberHandlers.GetMyProfileAsync)
            .WithName("GetMyMemberProfile")
            .RequireAuthorization()
            .Produces<MemberDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/active-bookings", MemberHandlers.GetByActiveBookingsAsync)
            .WithName("GetMembersByActiveBookings")
            .RequireAuthorization("StaffOnly")
            .Produces<ResultSet<MemberDto>>(StatusCodes.Status200OK);

        group.MapGet("/", MemberHandlers.GetAllAsync)
           .WithName("GetMembers")
           .RequireAuthorization("StaffOnly")
           .Produces<ResultSet<MemberDto>>(StatusCodes.Status200OK);

        // Command endpoints
        group.MapPost("/", MemberHandlers.CreateAsync)
            .WithName("CreateMember")
            .RequireAuthorization("StaffOnly")
            .Accepts<CreateMemberCommand>("application/json")
            .Produces<MemberDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", MemberHandlers.GetByIdAsync)
            .WithName("GetMemberById")
            .RequireAuthorization()
            .Produces<MemberDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", MemberHandlers.UpdateAsync)
            .WithName("UpdateMember")
            .RequireAuthorization()
            .Accepts<UpdateMemberCommand>("application/json")
            .Produces<MemberDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", MemberHandlers.DeleteAsync)
            .WithName("DeleteMember")
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;

    }
}
