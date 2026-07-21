using GymAppV3.Core.DTOs;
using GymAppV3.Core.Models;
using System;
using System.Linq.Expressions;

namespace GymAppV3.Infrastructure.Mappers
{
    internal static class GymBuildingMapper
    {
        // Address is an owned value object (no Id of its own), so it's flattened into
        // an AddressDto directly inside the same projection — still a single SQL SELECT.
        public static readonly Expression<Func<GymBuilding, GymBuildingDto>> ToDto =
            b => new GymBuildingDto(
                b.Id,
                b.Name,
                b.Description,
                new AddressDto(
                    b.Address.Street,
                    b.Address.City,
                    b.Address.State,
                    b.Address.ZipCode,
                    b.Address.Country),
                b.Phone,
                b.Email);

        public static readonly Func<GymBuilding, GymBuildingDto> ToDtoCompiled = ToDto.Compile();
    }
}
