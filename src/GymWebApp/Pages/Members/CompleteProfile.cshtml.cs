using FluentValidation;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymWebApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Members;

[Authorize(Policy = "MemberOnly")]
public class CompleteProfileModel : PageModel
{
    private readonly IMemberCommandService _memberCommandService;
    private readonly IValidator<CompleteMemberProfileCommand> _validator;

    public CompleteProfileModel(IMemberCommandService memberCommandService, IValidator<CompleteMemberProfileCommand> validator)
    {
        _memberCommandService = memberCommandService;
        _validator = validator;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    // Displayed read-only in the form. Kept as a page property (not ViewData) so it
    // survives POST re-renders when validation fails.
    public string Email { get; private set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string Firstname { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string Lastname { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Street Address")]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = "Greece";

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateOnly BirthDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(-20));

        [Display(Name = "I have medical conditions that may affect my training")]
        public bool HasMedicalConditions { get; set; }

        [StringLength(500)]
        [Display(Name = "Medical Conditions Details (optional)")]
        public string? MedicalConditionsDescription { get; set; }
    }

    public void OnGet()
    {
        // [Authorize(MemberOnly)] guarantees an authenticated Identity user, so Name is populated.
        Email = User.Identity!.Name!;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Email = User.Identity!.Name!;

        if (!ModelState.IsValid) return Page();

        var command = new CompleteMemberProfileCommand(
            Firstname: Input.Firstname,
            Lastname: Input.Lastname,
            Email: Email,
            Phone: Input.Phone,
            Address: new AddressDto(Input.Street, Input.City, Input.State, Input.ZipCode, Input.Country),
            BirthDate: Input.BirthDate,
            HasMedicalConditions: Input.HasMedicalConditions,
            MedicalNotes: Input.MedicalConditionsDescription
        );

        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return Page();
        }

        try
        {
            await _memberCommandService.CompleteProfileAsync(command, cancellationToken);
        }
        catch (BusinessRuleException ex)
        {
            // Expected domain rejection (e.g. profile already exists, underage) — show to user.
            // Everything else bubbles to the global exception handler.
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        TempData["SuccessMessage"] = "Your profile has been completed successfully!";
        return RedirectToPage("/Members/Dashboard");
    }
}