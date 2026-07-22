using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Bookings;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.Booking;

public static class BookingHandlers
{
    public static async Task<Created<BookingDto>> BookAsync(
        CreateBookingCommand command,
        IBookingCommandService commandService,
        CancellationToken cancellationToken)
    {
        var created = await commandService.BookAsync(command, cancellationToken);
        return TypedResults.Created($"/api/bookings/{created.Id}", created);
    }

    public static async Task<NoContent> CancelAsync(
        Guid id,
        IBookingCommandService commandService,
        CancellationToken cancellationToken)
    {
        await commandService.CancelAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<Ok<IReadOnlyList<BookingDto>>> GetByMemberAsync(
        Guid memberId,
        IBookingQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetByMemberAsync(new GetBookingsByMemberQuery(memberId), cancellationToken);
        return TypedResults.Ok(result);
    }
}
