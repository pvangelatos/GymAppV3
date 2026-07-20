using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints;

public static class GymBuildingEndpoints
{
    public static IEndpointRouteBuilder MapGymBuildingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/gym-buildings")
            .WithTags("Gym Buildings");

        group.MapGet("/", GymBuildingHandlers.GetAllAsync)
            .WithName("GetGymBuildings")
            .Produces<IReadOnlyList<GymBuildingDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GymBuildingHandlers.GetByIdAsync)
            .WithName("GetGymBuildingById")
            .Produces<GymBuildingDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", GymBuildingHandlers.CreateAsync)
            .WithName("CreateGymBuilding")
            .Accepts<CreateGymBuildingCommand>("application/json")
            .Produces<GymBuildingDto>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}", GymBuildingHandlers.UpdateAsync)
            .WithName("UpdateGymBuilding")
            .Accepts<UpdateGymBuildingCommand>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", GymBuildingHandlers.DeleteAsync)
            .WithName("DeleteGymBuilding")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
