using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.GymBuildings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Admin.Buildings;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly IGymBuildingQueryService _buildingQueryService;
    private readonly IGymBuildingCommandService _buildingCommandService;

    public EditModel(
        IGymBuildingQueryService buildingQueryService,
        IGymBuildingCommandService buildingCommandService)
    {
        _buildingQueryService = buildingQueryService;
        _buildingCommandService = buildingCommandService;
    }

    [BindProperty]
    public Guid BuildingId { get; set; }

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

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        BuildingId = id;

        var building = await _buildingQueryService.GetByIdAsync(
            new GetGymBuildingByIdQuery(id),
            cancellationToken);

        if (building == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Name = building.Name,
            Description = building.Description,
            Street = building.Address.Street,
            City = building.Address.City,
            State = building.Address.State,
            ZipCode = building.Address.ZipCode,
            Country = building.Address.Country,
            Phone = building.Phone,
            Email = building.Email
        };

        return Page();
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

        var command = new UpdateGymBuildingCommand(
            Input.Name,
            Input.Description,
            address,
            Input.Phone,
            Input.Email);

        await _buildingCommandService.UpdateAsync(BuildingId, command, cancellationToken);

        TempData["SuccessMessage"] = $"Building '{Input.Name}' updated successfully.";
        return RedirectToPage("./Index");
    }
}
