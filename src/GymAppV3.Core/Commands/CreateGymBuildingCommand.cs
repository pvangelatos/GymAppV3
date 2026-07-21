using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Commands
{
    // Input for creating a building. No Id — the server generates it. The address is
    // required because a building without a location makes no sense in the domain.
    public record CreateGymBuildingCommand(
        string Name,
        string? Description,
        AddressDto Address,
        string? Phone,
        string? Email);
}
