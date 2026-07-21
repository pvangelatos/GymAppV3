

namespace GymAppV3.Core.DTOs;

// Read model returned to clients. Includes the owning building's id so callers
// know which building the room belongs to, without embedding the whole building.
public record ClassRoomDto(
    Guid Id,
    string ClassRoomName,
    int Capacity,
    Guid GymBuildingId);
