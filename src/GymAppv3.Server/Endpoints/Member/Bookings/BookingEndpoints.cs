using GymAppv3.Server.Endpoints.Common;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints.Member.Bookings;

/// <summary>
/// Member-scoped booking endpoints.
/// Top-level booking operations are in Endpoints/Booking/.
/// </summary>
public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapMemberBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var memberBookings = app.MapGroup("/api/members/{memberId:guid}/bookings")
            .WithTags("Bookings");

        memberBookings.MapPost("/", BookingHandlers.BookAsync)
            .WithName("CreateBooking")
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<CreateBookingCommand>>()
            .Accepts<CreateBookingCommand>("application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<BookingDto>(StatusCodes.Status201Created);

        memberBookings.MapGet("/", BookingHandlers.GetByMemberAsync)
            .WithName("GetBookingsByMember")
            .RequireAuthorization()
            .Produces<ResultSet<BookingDto>>(StatusCodes.Status200OK);

        return app;
    }
}
