using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints.Trainer;

public static class TrainerEndpoints
{
    public static IEndpointRouteBuilder MapTrainerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/trainers")
            .WithTags("Trainers");

        group.MapGet("/me", TrainerHandlers.GetMyProfileAsync)
            .WithName("GetMyTrainerProfile")
            .RequireAuthorization()
            .Produces<TrainerDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", TrainerHandlers.GetAllAsync)
            .WithName("GetTrainers")
            .RequireAuthorization()
            .Produces<IReadOnlyList<TrainerDto>>(StatusCodes.Status200OK);

        group.MapPost("/", TrainerHandlers.CreateAsync)
            .WithName("CreateTrainer")
            .RequireAuthorization("AdminOnly")
            .Accepts<CreateTrainerCommand>("application/json")
            .Produces<TrainerCreatedDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", TrainerHandlers.GetByIdAsync)
            .WithName("GetTrainerById")
            .RequireAuthorization()
            .Produces<TrainerDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", TrainerHandlers.UpdateAsync)
            .WithName("UpdateTrainer")
            .RequireAuthorization()
            .Accepts<UpdateTrainerCommand>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", TrainerHandlers.DeleteAsync)
            .WithName("DeleteTrainer")
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
