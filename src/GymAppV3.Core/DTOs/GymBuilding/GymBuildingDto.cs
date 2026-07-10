using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.GymBuilding
{
    // Read model returned to clients. The owned Address is exposed as a nested DTO.
    // Audit and soft-delete fields are intentionally omitted from the public contract.
    public record GymBuildingDto(
        Guid Id,
        string Name,
        string? Description,
        AddressDto Address,
        string? Phone,
        string? Email);
}
