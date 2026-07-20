using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.ClassCategories;

namespace GymAppV3.Core.Interfaces;

public interface IClassCategoryQueryService
{
    Task<IReadOnlyList<ClassCategoryDto>> GetAllAsync(GetAllClassCategoriesQuery query, CancellationToken cancellationToken = default);
    Task<ClassCategoryDto?> GetByIdAsync(GetClassCategoryByIdQuery query, CancellationToken cancellationToken = default);
}
