using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.Trainers;

namespace GymAppV3.Core.Interfaces;

public interface ITrainerQueryService
{
    Task<IReadOnlyList<TrainerDto>> GetAllAsync(GetAllTrainersQuery query, CancellationToken cancellationToken = default);
    Task<TrainerDto?> GetByIdAsync(GetTrainerByIdQuery query, CancellationToken cancellationToken = default);
    Task<TrainerDto?> GetByUserIdAsync(GetTrainerByUserIdQuery query, CancellationToken cancellationToken = default);
}
