using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.ClassRoom
{
    // Input for updating a room. Id comes from the route. GymBuildingId is included
    // to allow moving a room to a different building, if that is permitted.
    public record UpdateClassRoomRequest(
        string ClassRoomName,
        int Capacity,
        Guid GymBuildingId);
}
