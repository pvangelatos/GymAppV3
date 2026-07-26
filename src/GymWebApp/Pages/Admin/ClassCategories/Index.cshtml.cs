using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.ClassCategories;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IClassCategoryQueryService _categoryQueryService;

    public IndexModel(IClassCategoryQueryService categoryQueryService)
    {
        _categoryQueryService = categoryQueryService;
    }

    public IReadOnlyList<ClassCategoryDto> Categories { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Categories = await _categoryQueryService.GetAllAsync(
            new GetAllClassCategoriesQuery(),
            cancellationToken);
    }
}