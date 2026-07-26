using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassCategories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.ClassCategory;

public static class ClassCategoryHandlers
{
    public static async Task<Ok<IReadOnlyList<ClassCategoryDto>>> GetAllAsync(
    IClassCategoryQueryService queryService,
    CancellationToken cancellationToken)
    {
        var result = await queryService.GetAllAsync(new GetAllClassCategoriesQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<ClassCategoryDto>, NotFound>> GetByIdAsync(
        Guid id,
        IClassCategoryQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetByIdAsync(new GetClassCategoryByIdQuery(id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Created<ClassCategoryDto>> CreateAsync(
    CreateClassCategoryCommand command,
    IClassCategoryCommandService commandService,
    CancellationToken cancellationToken)
    {
        var created = await commandService.CreateAsync(command, cancellationToken);
        return TypedResults.Created($"/api/class-categories/{created.Id}", created);
    }

    public static async Task<NoContent> UpdateAsync(
        Guid id,
        UpdateClassCategoryCommand command,
        IClassCategoryCommandService commandService,
        CancellationToken cancellationToken)
    {

        await commandService.UpdateAsync(id, command, cancellationToken);
        return TypedResults.NoContent();

    }

    public static async Task<NoContent> DeleteAsync(
        Guid id,
        IClassCategoryCommandService commandService,
        CancellationToken cancellationToken)
    {

        await commandService.DeleteAsync(id, cancellationToken);
        return TypedResults.NoContent();

    }
}
