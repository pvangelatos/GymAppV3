using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.GymBuildings;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints;

public static class GymBuildingHandlers
{
    public static async Task<Ok<IReadOnlyList<GymBuildingDto>>> GetAllAsync(
        IGymBuildingQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetAllAsync(new GetAllGymBuildingsQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<GymBuildingDto>, NotFound>> GetByIdAsync(
        Guid id,
        IGymBuildingQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetByIdAsync(new GetGymBuildingByIdQuery(id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Created<GymBuildingDto>> CreateAsync(
        CreateGymBuildingCommand command,
        IGymBuildingCommandService commandService,
        CancellationToken cancellationToken)
    {
        var created = await commandService.CreateAsync(command, cancellationToken);
        return TypedResults.Created($"/api/gym-buildings/{created.Id}", created);
    }

    public static async Task<Results<NoContent, NotFound>> UpdateAsync(
        Guid id,
        UpdateGymBuildingCommand command,
        IGymBuildingCommandService commandService,
        CancellationToken cancellationToken)
    {
        try
        {
            await commandService.UpdateAsync(id, command, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    public static async Task<Results<NoContent, NotFound>> DeleteAsync(
        Guid id,
        IGymBuildingCommandService commandService,
        CancellationToken cancellationToken)
    {
        try
        {
            await commandService.DeleteAsync(id, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
