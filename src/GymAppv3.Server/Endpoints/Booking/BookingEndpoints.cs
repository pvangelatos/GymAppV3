namespace GymAppv3.Server.Endpoints.Booking;

/// <summary>
/// Top-level booking endpoints (operations addressed by booking id).
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
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
