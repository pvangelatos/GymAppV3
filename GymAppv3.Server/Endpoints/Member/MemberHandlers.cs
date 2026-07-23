using GymAppV3.Core.Commands;
using GymAppV3.Core.Common;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Mvc;

namespace GymAppv3.Server.Endpoints.Member;

/// <summary>
/// Handler implementations for member endpoints.
/// </summary>
public static class MemberHandlers
{
    /// <summary>
    /// Gets a member by ID with authorization-aware medical notes.
    /// </summary>
    public static async Task<IResult> GetByIdAsync(
        Guid id,
        IMemberQueryService queryService,
        CancellationToken cancellationToken)
    {
        var query = new GetMemberByIdQuery(id);
        var member = await queryService.GetByIdAsync(query, cancellationToken);

        return member is not null
            ? Results.Ok(member)
            : Results.NotFound();
    }

    /// <summary>
    /// Gets all members with optional filtering and pagination.
    /// </summary>
    public static async Task<IResult> GetAllAsync(
        [AsParameters] GetAllMembersRequest request,
        IMemberQueryService queryService,
        CancellationToken cancellationToken)
    {
        var options = new ListOptions(request.Page ?? 1, request.Size ?? 50)
        {
            Sort = request.Sort
        };

        var query = new GetAllMembersQuery(
            Options: options,
            SearchTerm: request.SearchTerm,
            HasActiveMembership: request.HasActiveMembership);

        var result = await queryService.GetAllAsync(query, cancellationToken);

        return Results.Ok(result);
    }

    /// <summary>
    /// Gets members with active bookings (paginated).
    /// </summary>
    public static async Task<IResult> GetByActiveBookingsAsync(
        [AsParameters] PaginationRequest request,
        IMemberQueryService queryService,
        CancellationToken cancellationToken)
    {
        var options = new ListOptions(request.Page ?? 1, request.Size ?? 50)
        {
            Sort = request.Sort
        };

        var query = new GetMembersByActiveBookingsQuery(options);
        var result = await queryService.GetByActiveBookingsAsync(query, cancellationToken);

        return Results.Ok(result);
    }

    /// <summary>
    /// Gets members with active memberships (paginated).
    /// </summary>
    public static async Task<IResult> GetByActiveMembershipAsync(
        [AsParameters] PaginationRequest request,
        IMemberQueryService queryService,
        CancellationToken cancellationToken)
    {
        var options = new ListOptions(request.Page ?? 1, request.Size ?? 50)
        {
            Sort = request.Sort
        };

        var query = new GetMembersByActiveMembershipQuery(options);
        var result = await queryService.GetByActiveMembershipAsync(query, cancellationToken);

        return Results.Ok(result);
    }

    /// <summary>
    /// Creates a new member profile.
    /// </summary>
    public static async Task<IResult> CreateAsync(
        CreateMemberCommand command,
        IMemberCommandService commandService,
        CancellationToken cancellationToken)
    {
        var member = await commandService.CreateAsync(command, cancellationToken);

        return Results.Created($"/api/members/{member.Id}", member);
    }

    /// <summary>
    /// Updates an existing member profile.
    /// </summary>
    public static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateMemberCommand command,
        IMemberCommandService commandService,
        CancellationToken cancellationToken)
    {
        // Ensure ID in route matches command
        if (id != command.Id)
        {
            return Results.BadRequest("Member ID in URL does not match command.");
        }

        var member = await commandService.UpdateAsync(command, cancellationToken);

        return Results.Ok(member);
    }

    /// <summary>
    /// Soft-deletes a member and cascades to related entities.
    /// </summary>
    public static async Task<IResult> DeleteAsync(
        Guid id,
        IMemberCommandService commandService,
        CancellationToken cancellationToken)
    {
        var command = new DeleteMemberCommand(id);
        await commandService.DeleteAsync(command, cancellationToken);

        return Results.NoContent();
    }
}

/// <summary>
/// Request model for GetAll with filtering and pagination.
/// </summary>
public record GetAllMembersRequest(
    int? Page = 1,
    int? Size = 50,
    string? Sort = null,
    string? SearchTerm = null,
    bool? HasActiveMembership = null);

/// <summary>
/// Request model for basic pagination.
/// </summary>
public record PaginationRequest(
    int? Page = 1,
    int? Size = 50,
    string? Sort = null);
