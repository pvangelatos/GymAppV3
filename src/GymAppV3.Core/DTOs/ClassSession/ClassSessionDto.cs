using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.ClassSession
{
    // Read model for a class session. Includes the trainer and room *names* (not the
    // full entities) so a client can display the session without extra lookups, and
    // without pulling in unrelated or sensitive data from those entities.
    public record ClassSessionDto(
        Guid Id,
        string Title,
        DateTimeOffset StartsAt,
        int DurationInMinutes,
        int Capacity,
        int AvailableSeats,
        Guid TrainerId,
        string TrainerName,
        Guid ClassRoomId,
        string ClassRoomName);  
}
