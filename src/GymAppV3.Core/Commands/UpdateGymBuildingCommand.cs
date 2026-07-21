using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Commands;

// Input for updating a building. Id comes from the route, not the body.
public record UpdateGymBuildingCommand(
    string Name,
    string? Description,
    AddressDto Address,
    string? Phone,
    string? Email);
