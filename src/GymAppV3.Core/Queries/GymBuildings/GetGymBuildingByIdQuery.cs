using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.GymBuildings;

public record GetGymBuildingByIdQuery(Guid Id) : IQuery<GymBuildingDto?>;
