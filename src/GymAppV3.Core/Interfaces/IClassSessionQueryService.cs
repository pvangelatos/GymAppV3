using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.ClassSessions;

namespace GymAppV3.Core.Interfaces;

public interface IClassSessionQueryService
{
    Task<ClassSessionDto?> GetClassSessionByIdAsync(GetClassSessionByIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClassSessionDto>> GetUpcomingAsync(GetUpcomingClassSessionsQuery query, CancellationToken cancellationToken = default);
}
