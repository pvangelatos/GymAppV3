

namespace GymAppV3.Core.Commands;

// Input for creating a room. GymBuildingId ties the room to its building; the
// service should validate that the referenced building exists before inserting.
public record CreateClassRoomCommand(
    string ClassRoomName,
    int Capacity,
    Guid GymBuildingId);
