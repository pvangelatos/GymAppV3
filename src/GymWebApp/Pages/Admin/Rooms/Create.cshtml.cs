using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.GymBuildings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Admin.Rooms;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly IClassRoomCommandService _roomCommandService;
    private readonly IGymBuildingQueryService _buildingQueryService;

    public CreateModel(
        IClassRoomCommandService roomCommandService,
        IGymBuildingQueryService buildingQueryService)
    {
        _roomCommandService = roomCommandService;
        _buildingQueryService = buildingQueryService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public SelectList Buildings { get; set; } = default!;

    public class InputModel
    {
        [Required]
        [Display(Name = "Room Name")]
        [StringLength(100)]
        public string ClassRoomName { get; set; } = string.Empty;

        [Required]
        [Range(1, 500)]
        public int Capacity { get; set; }

        [Required]
        [Display(Name = "Building")]
        public Guid GymBuildingId { get; set; }
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadBuildingsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadBuildingsAsync(cancellationToken);
            return Page();
        }

        var command = new CreateClassRoomCommand(
            Input.ClassRoomName,
            Input.Capacity,
            Input.GymBuildingId);

        await _roomCommandService.CreateAsync(command, cancellationToken);

        TempData["SuccessMessage"] = $"Room '{Input.ClassRoomName}' created successfully.";
        return RedirectToPage("./Index");
    }

    private async Task LoadBuildingsAsync(CancellationToken cancellationToken)
    {
        var buildings = await _buildingQueryService.GetAllAsync(
            new GetAllGymBuildingsQuery(),
            cancellationToken);

        Buildings = new SelectList(buildings, nameof(GymBuildingDto.Id), nameof(GymBuildingDto.Name));
    }
}
