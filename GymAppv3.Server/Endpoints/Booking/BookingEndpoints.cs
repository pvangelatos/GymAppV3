using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints.Booking;

public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapPost("/", BookingHandlers.BookAsync)
            .WithName("CreateBooking")
            .RequireAuthorization()
            .Accepts<CreateBookingCommand>("application/json")
            .Produces<BookingDto>(StatusCodes.Status201Created);

        group.MapDelete("/{id:guid}", BookingHandlers.CancelAsync)
            .WithName("CancelBooking")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/member/{memberId:guid}", BookingHandlers.GetByMemberAsync)
            .WithName("GetBookingsByMember")
            .RequireAuthorization()
            .Produces<IReadOnlyList<BookingDto>>(StatusCodes.Status200OK);

        return app;
    }
}
