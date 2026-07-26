using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymWebApp.Pages.Admin.Trainers;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly ITrainerCommandService _trainerCommandService;
    private readonly IClassCategoryQueryService _categoryQueryService;

    public CreateModel(
        ITrainerCommandService trainerCommandService,
        IClassCategoryQueryService categoryQueryService)
    {
        _trainerCommandService = trainerCommandService;
        _categoryQueryService = categoryQueryService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public SelectList Categories { get; set; } = default!;

    public class InputModel
    {
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public List<Guid> SpecialtyCategoryIds { get; set; } = new();
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadCategoriesAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(cancellationToken);
            return Page();
        }

        var command = new CreateTrainerCommand(
            Input.Firstname,
            Input.Lastname,
            Input.Email,
            Input.Phone,
            Input.Bio,
            Input.SpecialtyCategoryIds);

        var result = await _trainerCommandService.CreateAsync(command, cancellationToken);

        TempData["SuccessMessage"] = $"Trainer {result.Trainer.Firstname} {result.Trainer.Lastname} created successfully. Temporary password: {result.TemporaryPassword}";
        return RedirectToPage("./Index");
    }

    private async Task LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await _categoryQueryService.GetAllAsync(
            new GetAllClassCategoriesQuery(),
            cancellationToken);

        Categories = new SelectList(categories, nameof(ClassCategoryDto.Id), nameof(ClassCategoryDto.Name));
    }
}
