using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.ClassSessions;

public record GetClassSessionByIdQuery(Guid Id) : IQuery<ClassSessionDto?>;
