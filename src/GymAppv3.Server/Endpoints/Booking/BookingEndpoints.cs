using GymAppv3.Server.Endpoints.Common;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.Booking;

/// <summary>
/// Top-level booking endpoints (admin operations for all bookings).
/// Member-scoped booking routes are in Member/Bookings/.
/// </summary>
public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapPost("/{id:guid}/cancel", BookingHandlers.CancelAsync)
            .WithName("CancelBooking")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
