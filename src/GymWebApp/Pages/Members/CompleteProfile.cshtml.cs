using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Members;

[Authorize]
public class CompleteProfileModel : PageModel
{
    private readonly IMemberCommandService _memberCommandService;

    public CompleteProfileModel(IMemberCommandService memberCommandService)
    {
        _memberCommandService = memberCommandService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

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
        // Pre-populate with user's email
        var email = User.Identity?.Name;
        if (!string.IsNullOrEmpty(email))
        {
            ViewData["Email"] = email;
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.Identity?.Name;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
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

            var command = new CompleteMemberProfileCommand(
                Firstname: Input.Firstname,
                Lastname: Input.Lastname,
                Email: email,
                Phone: Input.Phone,
                Address: address,
                BirthDate: Input.BirthDate,
                HasMedicalConditions: Input.HasMedicalConditions,
                MedicalNotes: Input.MedicalConditionsDescription
            );

            await _memberCommandService.CompleteProfileAsync(command, cancellationToken);

            TempData["SuccessMessage"] = "Your profile has been completed successfully!";
            return RedirectToPage("/Members/Dashboard");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return Page();
        }
    }
}
