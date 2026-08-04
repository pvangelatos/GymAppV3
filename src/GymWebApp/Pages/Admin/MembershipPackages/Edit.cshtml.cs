using FluentValidation;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassCategories;
using GymAppV3.Core.Queries.MembershipPackages;
using GymWebApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Admin.MembershipPackages;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly IMembershipPackageQueryService _packageQueryService;
    private readonly IMembershipPackageCommandService _packageCommandService;
    private readonly IClassCategoryQueryService _categoryQueryService;
    private readonly IValidator<UpdateMembershipPackageCommand> _validator;

    public EditModel(
        IMembershipPackageQueryService packageQueryService,
        IMembershipPackageCommandService packageCommandService,
        IClassCategoryQueryService categoryQueryService,
        IValidator<UpdateMembershipPackageCommand> validator)
    {
        _packageQueryService = packageQueryService;
        _packageCommandService = packageCommandService;
        _categoryQueryService = categoryQueryService;
        _validator = validator;
    }

    [BindProperty]
    public Guid PackageId { get; set; }

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

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        PackageId = id;

        var package = await _packageQueryService.GetByIdAsync(
            new GetMembershipPackageByIdQuery(id),
            cancellationToken);

        if (package == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Name = package.Name,
            Price = package.Price,
            DurationInDays = package.DurationInDays,
            SessionsIncluded = package.SessionsIncluded,
            ClassCategoryId = package.ClassCategoryId
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

        var command = new UpdateMembershipPackageCommand(
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

        await _packageCommandService.UpdateAsync(PackageId, command, cancellationToken);

        TempData["SuccessMessage"] = $"Membership package '{Input.Name}' updated successfully.";
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
