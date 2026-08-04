using FluentValidation;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymWebApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Staff.Members;

[Authorize(Policy = "StaffOnly")]
public class CreateModel : PageModel
{
    private readonly IMemberCommandService _memberCommandService;
    private readonly IValidator<CreateMemberCommand> _validator;


    public CreateModel(
        IMemberCommandService memberCommandService,
        IValidator<CreateMemberCommand> validator)
    {
        _memberCommandService = memberCommandService;
        _validator = validator;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(50)]
        public string Firstname { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Lastname { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        [Required]
        [StringLength(100)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = "Greece";

        [Required]
        [DataType(DataType.Date)]
        public DateOnly BirthDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(-20));

        public bool HasMedicalConditions { get; set; }

        [StringLength(500)]
        public string? MedicalNotes { get; set; }
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

        try
        {
            var address = new AddressDto(
                Input.Street,
                Input.City,
                Input.State,
                Input.ZipCode,
                Input.Country
            );

            var command = new CreateMemberCommand(
                Firstname: Input.Firstname,
                Lastname: Input.Lastname,
                Email: Input.Email,
                Phone: Input.Phone,
                Address: address,
                BirthDate: Input.BirthDate,
                HasMedicalConditions: Input.HasMedicalConditions,
                MedicalNotes: Input.MedicalNotes
            );

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return Page();
            }

            await _memberCommandService.CreateAsync(command, cancellationToken);

            TempData["SuccessMessage"] = "Member created successfully!";
            return RedirectToPage("/Staff/Members/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return Page();
        }
    }
}
