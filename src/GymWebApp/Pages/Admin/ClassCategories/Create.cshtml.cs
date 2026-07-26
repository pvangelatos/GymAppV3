using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Admin.ClassCategories;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly IClassCategoryCommandService _categoryCommandService;

    public CreateModel(IClassCategoryCommandService categoryCommandService)
    {
        _categoryCommandService = categoryCommandService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        [Required]
        [Display(Name = "Category Name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var command = new CreateClassCategoryCommand(Input.Name);
        await _categoryCommandService.CreateAsync(command, cancellationToken);

        TempData["SuccessMessage"] = $"Class category '{Input.Name}' created successfully.";
        return RedirectToPage("./Index");
    }
}
