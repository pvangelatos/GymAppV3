using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GymAppv3.Server.Endpoints.Member;

/// <summary>
/// Handler implementations for member endpoints.
/// </summary>
public static class MemberHandlers
{
    // --- Self-service ---------------------------------------------------------

    /// <summary>
    /// Completes a member's profile.
    /// </summary>
    /// <param name="command">The command containing the member details.</param>
    /// <param name="commandService">The command service to handle the completion.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created member details.</returns>
    public static async Task<Created<MemberDto>> CompleteProfileAsync(
        CompleteMemberProfileCommand command,
        IMemberCommandService commandService,
        CancellationToken cancellationToken)
    {
        // UserId is read from the token inside the service — never from the body.
        var created = await commandService.CompleteProfileAsync(command, cancellationToken);
        return TypedResults.Created($"/api/members/{created.Id}", created);
    }

    /// <summary>
    /// Gets the profile of the currently authenticated user.
    /// </summary>
    /// <param name="queryService">The query service to handle the retrieval.</param>
    /// <param name="userContext">The user context containing the current user's information.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The profile of the currently authenticated user, or a 404 if not found.</returns>
    public static async Task<Results<Ok<MemberDto>, NotFound>> GetMyProfileAsync(
        IMemberQueryService queryService,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return TypedResults.NotFound();

        // 404 here means "registered but step 2 not done yet".
        var result = await queryService.GetByUserIdAsync(
            new GetMemberByUserIdQuery(userId), cancellationToken);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    // --- Staff-created profiles -----------------------------------------------

    /// <summary>
    /// Creates a new member profile.
    /// </summary>  
    /// <param name="command">The command containing the member details.</param>
    /// <param name="commandService">The command service to handle the creation.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created member details.</returns>
    public static async Task<Created<MemberDto>> CreateAsync(
        CreateMemberCommand command,
        IMemberCommandService commandService,
        CancellationToken cancellationToken)
    {
        var created = await commandService.CreateAsync(command, cancellationToken);
        return TypedResults.Created($"/api/members/{created.Id}", created);
    }

    // --- Listings -------------------------------------------------------------

    /// <summary>
    /// Gets all members with optional filtering and pagination.
    /// </summary>
    /// <param name="request">The request containing filtering and pagination options.</param>
    /// <param name="queryService">The query service to handle the retrieval.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A paginated list of members.</returns>
    public static async Task<Ok<ResultSet<MemberDto>>> GetAllAsync(
        [AsParameters] GetAllMembersRequest request,
        IMemberQueryService queryService,
        CancellationToken cancellationToken)
    {
        var options = new ListOptions(request.Page, request.Size) { Sort = request.Sort };

        var result = await queryService.GetAllAsync(
            new GetAllMembersQuery(options, request.SearchTerm, request.HasActiveMembership), cancellationToken);

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets members with active bookings (paginated).
    /// </summary>
    /// <param name="request">The request containing filtering and pagination options.</param>      
    /// <param name="queryService">The query service to handle the retrieval.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A paginated list of members with active bookings.</returns>
    public static async Task<Ok<ResultSet<MemberDto>>> GetByActiveBookingsAsync(
        [AsParameters] GetAllMembersRequest request,
        IMemberQueryService queryService,
        CancellationToken cancellationToken)
    {
        var options = new ListOptions(request.Page, request.Size) { Sort = request.Sort };

        var result = await queryService.GetByActiveBookingsAsync(
            new GetMembersByActiveBookingsQuery(options), cancellationToken);

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Gets members with active memberships (paginated).
    /// </summary>
    /// <param name="request">The request containing filtering and pagination options.</param>
    /// <param name="queryService">The query service to handle the retrieval.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A paginated list of members with active memberships.</returns>
    public static async Task<Ok<ResultSet<MemberDto>>> GetByActiveMembershipAsync(
        [AsParameters] GetAllMembersRequest request,
        IMemberQueryService queryService,
        CancellationToken cancellationToken)
    {
        var options = new ListOptions(request.Page, request.Size) { Sort = request.Sort };

        var result = await queryService.GetByActiveMembershipAsync(
            new GetMembersByActiveMembershipQuery(options), cancellationToken);

        return TypedResults.Ok(result);
    }

    // --- Single member --------------------------------------------------------

    /// <summary>
    /// Gets a member by ID with authorization-aware medical notes.
    /// </summary>
    /// <param name="id">The ID of the member to retrieve.</param>
    /// <param name="queryService">The query service to handle the retrieval.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The member details if found; otherwise, a NotFound result.</returns>
    public static async Task<Results<Ok<MemberDetailDto>, NotFound>> GetByIdAsync(
        Guid id,
        IMemberQueryService queryService,
        CancellationToken cancellationToken)
    {
        // MedicalNotes comes back null unless the caller passes the contextual check.
        var result = await queryService.GetByIdAsync(
            new GetMemberByIdQuery(id), cancellationToken);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
    /// <summary>
    /// Updates an existing member profile.
    /// </summary>
    /// <param name="id">The ID of the member to update.</param>
    /// <param name="command">The update command containing the new member data.</param>
    /// <param name="commandService">The command service to handle the update.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The updated member.</returns>
    public static async Task<Ok<MemberDto>> UpdateAsync(
        Guid id,
        UpdateMemberCommand command,
        IMemberCommandService commandService,
        CancellationToken cancellationToken)
    {
        // The route decides which member is updated; any Id in the body is ignored.
        var result = await commandService.UpdateAsync(command with { Id = id }, cancellationToken);
        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Deletes a member by ID.
    /// </summary>
    /// <param name="id">The ID of the member to delete.</param>
    /// <param name="commandService">The command service to handle the deletion.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A NoContent result if the deletion is successful.</returns>
    public static async Task<NoContent> DeleteAsync(
        Guid id,
        IMemberCommandService commandService,
        CancellationToken cancellationToken)
    {
        await commandService.DeleteAsync(new DeleteMemberCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }    
}

/// <summary>
/// Request model for GetAll with filtering and pagination.
/// </summary>
public record GetAllMembersRequest(
    int Page = 1,
    int Size = 50,
    string? Sort = null,
    string? SearchTerm = null,
    bool? HasActiveMembership = null);

/// <summary>
/// Request model for basic pagination.
/// </summary>
public record PaginationRequest(
    int Page = 1,
    int Size = 50,
    string? Sort = null);
