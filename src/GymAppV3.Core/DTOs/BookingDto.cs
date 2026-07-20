

namespace GymAppV3.Core.DTOs
{
    // Read model for a booking. Includes the session title and start time so a client
    // can display the booking without a second lookup.
    public record BookingDto(
        Guid Id,
        Guid MemberId,
        Guid ClassSessionId,
        string SessionTitle,
        DateTimeOffset SessionStartsAt,
        string Status,
        DateTimeOffset BookedAt,
        DateTimeOffset? CancelledAt);
}
