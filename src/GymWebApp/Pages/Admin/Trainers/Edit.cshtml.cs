using FluentValidation;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassCategories;
using GymAppV3.Core.Queries.Trainers;
using GymWebApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymWebApp.Pages.Admin.Trainers;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly ITrainerQueryService _trainerQueryService;
    private readonly ITrainerCommandService _trainerCommandService;
    private readonly IClassCategoryQueryService _categoryQueryService;
    private readonly IValidator<UpdateTrainerCommand> _validator;

    public EditModel(
        ITrainerQueryService trainerQueryService,
        ITrainerCommandService trainerCommandService,
        IClassCategoryQueryService categoryQueryService,
        IValidator<UpdateTrainerCommand> validator)
    {
        _trainerQueryService = trainerQueryService;
        _trainerCommandService = trainerCommandService;
        _categoryQueryService = categoryQueryService;
        _validator = validator;
    }

    [BindProperty]
    public Guid TrainerId { get; set; }

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

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        TrainerId = id;

        var trainer = await _trainerQueryService.GetByIdAsync(
            new GetTrainerByIdQuery(id),
            cancellationToken);

        if (trainer == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Firstname = trainer.Firstname,
            Lastname = trainer.Lastname,
            Email = trainer.Email,
            Phone = trainer.Phone,
            Bio = trainer.Bio,
            SpecialtyCategoryIds = trainer.Specialties.Select(s => s.ClassCategoryId).ToList()
        };

        await LoadCategoriesAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(cancellationToken);
            return Page();
        }

        var command = new UpdateTrainerCommand(
            Input.Firstname,
            Input.Lastname,
            Input.Email,
            Input.Phone,
            Input.Bio,
            Input.SpecialtyCategoryIds);

        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            await LoadCategoriesAsync(cancellationToken);
            return Page();
        }

        await _trainerCommandService.UpdateAsync(TrainerId, command, cancellationToken);

        TempData["SuccessMessage"] = $"Trainer {Input.Firstname} {Input.Lastname} updated successfully.";
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
