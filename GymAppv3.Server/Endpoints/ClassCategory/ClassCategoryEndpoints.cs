using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints.ClassCategory
{
    public static class ClassCategoryEndpoints
    {
        public static IEndpointRouteBuilder MapClassCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/class-categories")
                .WithTags("Class Categories");

            group.MapGet("/", ClassCategoryHandlers.GetAllAsync)
                .WithName("GetClassCategories")
                .Produces<IReadOnlyList<ClassCategoryDto>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", ClassCategoryHandlers.GetByIdAsync)
            .WithName("GetClassCategoryById")
            .Produces<ClassCategoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/", ClassCategoryHandlers.CreateAsync)
                .WithName("CreateClassCategory")
                .Accepts<CreateClassCategoryCommand>("application/json")
                .Produces<ClassCategoryDto>(StatusCodes.Status201Created);

            group.MapPut("/{id:guid}", ClassCategoryHandlers.UpdateAsync)
                .WithName("UpdateClassCategory")
                .Accepts<UpdateClassCategoryCommand>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete("/{id:guid}", ClassCategoryHandlers.DeleteAsync)
                .WithName("DeleteClassCategory")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}
