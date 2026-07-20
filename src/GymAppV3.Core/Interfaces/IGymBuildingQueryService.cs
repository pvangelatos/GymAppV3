using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.GymBuildings;

namespace GymAppV3.Core.Interfaces;

public interface IGymBuildingQueryService
{
    Task<IReadOnlyList<GymBuildingDto>> GetAllAsync(GetAllGymBuildingsQuery query, CancellationToken cancellationToken = default);
    Task<GymBuildingDto?> GetByIdAsync(GetGymBuildingByIdQuery query, CancellationToken cancellationToken = default);
}
