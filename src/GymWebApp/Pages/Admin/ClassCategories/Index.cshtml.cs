using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.ClassCategories;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IClassCategoryQueryService _categoryQueryService;
    private readonly IClassCategoryCommandService _categoryCommandService;

    public IndexModel(IClassCategoryQueryService categoryQueryService, IClassCategoryCommandService categoryCommandService)
    {
        _categoryQueryService = categoryQueryService;
        _categoryCommandService = categoryCommandService;
    }

    public IReadOnlyList<ClassCategoryDto> Categories { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Categories = await _categoryQueryService.GetAllAsync(
            new GetAllClassCategoriesQuery(),
            cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _categoryCommandService.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Category deleted successfully.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage("./Index");
    }
}