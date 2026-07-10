using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.ClassRoom
{
    // Input for creating a room. GymBuildingId ties the room to its building; the
    // service should validate that the referenced building exists before inserting.
    public record CreateClassRoomRequest(
        string ClassRoomName,
        int Capacity,
        Guid GymBuildingId);
}
