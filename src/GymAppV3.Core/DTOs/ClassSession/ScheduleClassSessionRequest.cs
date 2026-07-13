using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.ClassSession
{
    // Input for scheduling a new class session. Notice what is absent: Id (system
    // generates it), AvailableSeats (starts equal to Capacity — no bookings yet),
    // and RowVersion (managed by EF). The client only supplies real decisions.
    public record ScheduleClassSessionRequest(
        string Title,
        DateTimeOffset StartsAt,
        int DurationInMinutes,
        int Capacity,
        Guid TrainerId,
        Guid ClassRoomId);
}
