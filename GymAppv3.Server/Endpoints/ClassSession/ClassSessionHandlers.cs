using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassSessions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.ClassSession;

public static class ClassSessionHandlers
{
    public static async Task<Ok<IReadOnlyList<ClassSessionDto>>> GetUpcomingAsync(
        IClassSessionQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetUpcomingAsync(new GetUpcomingClassSessionsQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<ClassSessionDto>, NotFound>> GetByIdAsync(
        Guid id, 
        IClassSessionQueryService queryService, 
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetClassSessionByIdAsync(new GetClassSessionByIdQuery(id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Created<ClassSessionDto>> ScheduleAsync(
        ScheduleClassSessionCommand command,
        IClassSessionCommandService commandService,
        CancellationToken cancellationToken)
    {
        var created = await commandService.ScheduleAsync(command, cancellationToken);
        return TypedResults.Created($"/api/class-sessions/{created.Id}", created);
    }
}

