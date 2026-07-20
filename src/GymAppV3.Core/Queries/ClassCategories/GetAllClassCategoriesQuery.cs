using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.ClassCategories;

public record GetAllClassCategoriesQuery : IQuery<IReadOnlyList<ClassCategoryDto>>;
