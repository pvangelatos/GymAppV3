using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces;

public interface ITrainerCommandService
{
    Task<TrainerCreatedDto> CreateAsync(CreateTrainerCommand command, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateTrainerCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
