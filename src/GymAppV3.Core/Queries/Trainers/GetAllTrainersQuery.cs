using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.Trainers;

public record GetAllTrainersQuery : IQuery<IReadOnlyList<TrainerDto>>;

