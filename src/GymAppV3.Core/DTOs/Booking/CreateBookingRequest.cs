using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.Booking
{
    // Input for booking a session. The client supplies only who and which session —
    // the service finds the right membership to charge (same category, with balance).
    public record CreateBookingRequest(
        Guid MemberId,
        Guid ClassSessionId);
}
