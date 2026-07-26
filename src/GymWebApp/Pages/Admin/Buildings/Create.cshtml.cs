using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Admin.Buildings;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly IGymBuildingCommandService _buildingCommandService;

    public CreateModel(IGymBuildingCommandService buildingCommandService)
    {
        _buildingCommandService = buildingCommandService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        [Required]
        [Display(Name = "Building Name")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Street Address")]
        public string Street { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
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

        var address = new AddressDto(
            Input.Street,
            Input.City,
            Input.State,
            Input.ZipCode,
            Input.Country);

        var command = new CreateGymBuildingCommand(
            Input.Name,
            Input.Description,
            address,
            Input.Phone,
            Input.Email);

        await _buildingCommandService.CreateAsync(command, cancellationToken);

        TempData["SuccessMessage"] = $"Building '{Input.Name}' created successfully.";
        return RedirectToPage("./Index");
    }
}
