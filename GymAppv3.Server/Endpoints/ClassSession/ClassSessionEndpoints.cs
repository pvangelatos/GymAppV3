using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints.ClassSession;

public static class ClassSessionEndpoints
{
    public static IEndpointRouteBuilder MapClassSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/class-sessions")
            .WithTags("Class Sessions");

        group.MapGet("/upcoming", ClassSessionHandlers.GetUpcomingAsync)
            .WithName("GetUpcomingClassSessions")
            .RequireAuthorization()
            .Produces<IReadOnlyList<ClassSessionDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", ClassSessionHandlers.GetByIdAsync)
            .WithName("GetClassSessionById")
            .RequireAuthorization()
            .Produces<ClassSessionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", ClassSessionHandlers.ScheduleAsync)
            .WithName("ScheduleClassSession")
            .RequireAuthorization("AdminOnly")
            .Accepts<ScheduleClassSessionCommand>("application/json")
            .Produces<ClassSessionDto>(StatusCodes.Status201Created);

        return app;
    }
}
