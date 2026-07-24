using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.Trainers;

public record GetTrainerByIdQuery(Guid Id) : IQuery<TrainerDto?>;
