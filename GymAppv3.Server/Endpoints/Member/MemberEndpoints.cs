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

        // Query endpoints
        group.MapGet("/{id:guid}", MemberHandlers.GetByIdAsync)
            .WithName("GetMemberById")
            .WithSummary("Get member by ID")
            .WithDescription("Returns member details. Medical notes visibility depends on authorization.")
            .Produces<MemberDetailDto>()
            .Produces(404)
            .RequireAuthorization();

        group.MapGet("/", MemberHandlers.GetAllAsync)
            .WithName("GetAllMembers")
            .WithSummary("Get all members with filtering and pagination")
            .WithDescription("Admin/Trainer only. Supports search by name/email and active membership filter.")
            .Produces<ResultSet<MemberDto>>()
            .RequireAuthorization("AdminOrTrainer");

        group.MapGet("/active-bookings", MemberHandlers.GetByActiveBookingsAsync)
            .WithName("GetMembersByActiveBookings")
            .WithSummary("Get members with active bookings")
            .WithDescription("Admin/Trainer only. Returns paginated list of members with future bookings.")
            .Produces<ResultSet<MemberDto>>()
            .RequireAuthorization("AdminOrTrainer");

        group.MapGet("/active-memberships", MemberHandlers.GetByActiveMembershipAsync)
            .WithName("GetMembersByActiveMemberships")
            .WithSummary("Get members with active memberships")
            .WithDescription("Admin/Trainer only. Returns paginated list of members with currently active memberships.")
            .Produces<ResultSet<MemberDto>>()
            .RequireAuthorization("AdminOrTrainer");

        // Command endpoints
        group.MapPost("/", MemberHandlers.CreateAsync)
            .WithName("CreateMember")
            .WithSummary("Create a new member profile")
            .WithDescription("Admin/Trainer can create members without user accounts. Validates minimum age and email uniqueness.")
            .Produces<MemberDto>(201)
            .Produces(400)
            .RequireAuthorization("AdminOrTrainer");

        group.MapPut("/{id:guid}", MemberHandlers.UpdateAsync)
            .WithName("UpdateMember")
            .WithSummary("Update member profile")
            .WithDescription("Member can update own profile, Admin can update any. Email changes sync to IdentityUser.")
            .Produces<MemberDto>()
            .Produces(400)
            .Produces(404)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", MemberHandlers.DeleteAsync)
            .WithName("DeleteMember")
            .WithSummary("Soft-delete a member")
            .WithDescription("Admin only. Cascades soft-delete to memberships, cancels bookings, preserves payments.")
            .Produces(204)
            .Produces(404)
            .RequireAuthorization("AdminOnly");

        return app;
    }
}
