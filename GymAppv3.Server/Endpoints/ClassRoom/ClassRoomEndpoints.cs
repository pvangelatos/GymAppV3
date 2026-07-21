using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints.ClassRoom;

public static class ClassRoomEndpoints
{
    public static IEndpointRouteBuilder MapClassRoomEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/class-rooms")
            .WithTags("Class Rooms");

        group.MapGet("/", ClassRoomHandlers.GetAllAsync)
            .WithName("GetClassRooms")
            .Produces<IReadOnlyList<ClassRoomDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", ClassRoomHandlers.GetByIdAsync)
            .WithName("GetClassRoomById")
            .Produces<ClassRoomDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", ClassRoomHandlers.CreateAsync)
            .WithName("CreateClassRoom")
            .Accepts<CreateClassRoomCommand>("application/json")
            .Produces<ClassRoomDto>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}", ClassRoomHandlers.UpdateAsync)
            .WithName("UpdateClassRoom")
            .Accepts<UpdateClassRoomCommand>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", ClassRoomHandlers.DeleteAsync)
            .WithName("DeleteClassRoom")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
