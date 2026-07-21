using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassRooms;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.ClassRoom;

public static class ClassRoomHandlers
{
    public static async Task<Ok<IReadOnlyList<ClassRoomDto>>> GetAllAsync(
        IClassRoomQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetAllAsync(new GetAllClassRoomsQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<ClassRoomDto>, NotFound>> GetByIdAsync(
        Guid id,
        IClassRoomQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetByIdAsync(new GetClassRoomByIdQuery(id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Created<ClassRoomDto>> CreateAsync(
        CreateClassRoomCommand command,
        IClassRoomCommandService commandService,
        CancellationToken cancellationToken)
    {
        var created = await commandService.CreateAsync(command, cancellationToken);
        return TypedResults.Created($"/api/class-rooms/{created.Id}", created);
    }

    public static async Task<NoContent> UpdateAsync(
        Guid id,
        UpdateClassRoomCommand command,
        IClassRoomCommandService commandService,
        CancellationToken cancellationToken)
    {
        await commandService.UpdateAsync(id, command, cancellationToken);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteAsync(
        Guid id,
        IClassRoomCommandService commandService,
        CancellationToken cancellationToken)
    {
        await commandService.DeleteAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }
}

