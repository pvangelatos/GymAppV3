using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Trainers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.Trainer;

public static class TrainerHandlers
{
    public static async Task<Created<TrainerCreatedDto>> CreateAsync(
        CreateTrainerCommand command,
        ITrainerCommandService commandService,
        CancellationToken cancellationToken)
    {
        // The response carries the one-time password — never log this result.
        var created = await commandService.CreateAsync(command, cancellationToken);
        return TypedResults.Created($"/api/trainers/{created.Trainer.Id}", created);
    }

    public static async Task<Ok<IReadOnlyList<TrainerDto>>> GetAllAsync(
        ITrainerQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetAllAsync(new GetAllTrainersQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<TrainerDto>, NotFound>> GetMyProfileAsync(
        ITrainerQueryService queryService,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        if (string.IsNullOrEmpty(userId))
            return TypedResults.NotFound();

        // 404 means the caller is authenticated but isn't a trainer.
        var result = await queryService.GetByUserIdAsync(
            new GetTrainerByUserIdQuery(userId), cancellationToken);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<TrainerDto>, NotFound>> GetByIdAsync(
        Guid id,
        ITrainerQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetByIdAsync(new GetTrainerByIdQuery(id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<NoContent> UpdateAsync(
        Guid id,
        UpdateTrainerCommand command,
        ITrainerCommandService commandService,
        CancellationToken cancellationToken)
    {
        await commandService.UpdateAsync(id, command, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteAsync(
        Guid id,
        ITrainerCommandService commandService,
        CancellationToken cancellationToken)
    {
        await commandService.DeleteAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }
}
