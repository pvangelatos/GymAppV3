using GymAppv3.Server.Endpoints.Common;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Bookings;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.Member.Bookings;

/// <summary>
/// Handlers for member-scoped booking endpoints.
/// </summary>
public static class BookingHandlers
{
    public static async Task<Created<BookingDto>> BookAsync(
    Guid memberId,
    CreateBookingCommand command,
    IBookingCommandService commandService,
    CancellationToken cancellationToken)
    {
        // The route is the source of truth. MemberScopedFilter already rejects a
        // mismatching body; overriding here keeps us safe even if the filter is removed.
        var created = await commandService.BookAsync(command with { MemberId = memberId }, cancellationToken);
        return TypedResults.Created($"/api/bookings/{created.Id}", created);
    }

    public static async Task<Ok<ResultSet<BookingDto>>> GetByMemberAsync(
        Guid memberId,
        [AsParameters] PaginationRequest request,
        IBookingQueryService queryService,
        CancellationToken cancellationToken)
    {
        var options = new ListOptions(request.Page, request.Size) { Sort = request.Sort };

        var result = await queryService.GetByMemberAsync(
            new GetBookingsByMemberQuery(memberId, options), cancellationToken);

        return TypedResults.Ok(result);
    }
}
