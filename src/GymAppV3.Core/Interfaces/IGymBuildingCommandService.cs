using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces;

public interface IGymBuildingCommandService
{
    Task<GymBuildingDto> CreateAsync(CreateGymBuildingCommand command, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateGymBuildingCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
