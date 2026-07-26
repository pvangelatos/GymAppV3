using GymAppV3.Core.Commands;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Admin.ClassCategories;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly IClassCategoryQueryService _categoryQueryService;
    private readonly IClassCategoryCommandService _categoryCommandService;

    public EditModel(
        IClassCategoryQueryService categoryQueryService,
        IClassCategoryCommandService categoryCommandService)
    {
        _categoryQueryService = categoryQueryService;
        _categoryCommandService = categoryCommandService;
    }

    [BindProperty]
    public Guid CategoryId { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        [Required]
        [Display(Name = "Category Name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        CategoryId = id;

        var category = await _categoryQueryService.GetByIdAsync(
            new GetClassCategoryByIdQuery(id),
            cancellationToken);

        if (category == null)
        {
            return NotFound();
        }

        Input = new InputModel { Name = category.Name };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var command = new UpdateClassCategoryCommand(Input.Name);
        await _categoryCommandService.UpdateAsync(CategoryId, command, cancellationToken);

        TempData["SuccessMessage"] = $"Class category '{Input.Name}' updated successfully.";
        return RedirectToPage("./Index");
    }
}
