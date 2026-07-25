using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.ClassSessions;

public record GetUpcomingClassSessionsQuery(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null) : IQuery<IReadOnlyList<ClassSessionDto>>;
