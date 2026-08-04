using FluentValidation;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassCategories;
using GymWebApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Admin.MembershipPackages;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly IMembershipPackageCommandService _packageCommandService;
    private readonly IClassCategoryQueryService _categoryQueryService;
    private readonly IValidator<CreateMembershipPackageCommand> _validator;


    public CreateModel(
        IMembershipPackageCommandService packageCommandService,
        IClassCategoryQueryService categoryQueryService,
        IValidator<CreateMembershipPackageCommand> validator)
    {
        _packageCommandService = packageCommandService;
        _categoryQueryService = categoryQueryService;
        _validator = validator;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public SelectList Categories { get; set; } = default!;

    public class InputModel
    {
        [Required]
        [Display(Name = "Package Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Duration (Days)")]
        [Range(1, 365)]
        public int DurationInDays { get; set; }

        [Required]
        [Display(Name = "Sessions Included")]
        [Range(1, 1000)]
        public int SessionsIncluded { get; set; }

        [Required]
        [Display(Name = "Class Category")]
        public Guid ClassCategoryId { get; set; }
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

        var command = new CreateMembershipPackageCommand(
            Input.Name,
            Input.Price,
            Input.DurationInDays,
            Input.SessionsIncluded,
            Input.ClassCategoryId);

        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            await LoadCategoriesAsync(cancellationToken);
            return Page();
        }

        await _packageCommandService.CreateAsync(command, cancellationToken);

        TempData["SuccessMessage"] = $"Membership package '{Input.Name}' created successfully.";
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
