using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.GymBuilding
{
    // Input for creating a building. No Id — the server generates it. The address is
    // required because a building without a location makes no sense in the domain.
    public record CreateGymBuildingRequest(
        string Name,
        string? Description,
        AddressDto Address,
        string? Phone,
        string? Email);
}
