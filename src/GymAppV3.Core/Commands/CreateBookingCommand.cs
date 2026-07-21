

namespace GymAppV3.Core.Commands
{
    // Input for booking a session. The client supplies only who and which session —
    // the service finds the right membership to charge (same category, with balance).
    public record CreateBookingCommand(
        Guid MemberId,
        Guid ClassSessionId);
}
