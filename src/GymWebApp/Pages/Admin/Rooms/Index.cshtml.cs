using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassRooms;
using GymAppV3.Core.Queries.GymBuildings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.Rooms;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IClassRoomQueryService _roomQueryService;
    private readonly IClassRoomCommandService _roomCommandService;
    private readonly IGymBuildingQueryService _buildingQueryService;

    public IndexModel(IClassRoomQueryService roomQueryService, IClassRoomCommandService roomCommandService, IGymBuildingQueryService buildingQueryService)
    {
        _roomQueryService = roomQueryService;
        _roomCommandService = roomCommandService;
        _buildingQueryService = buildingQueryService;
    }

    public IReadOnlyList<ClassRoomDto> Rooms { get; set; } = [];

    public Dictionary<Guid, string> BuildingNames { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Rooms = await _roomQueryService.GetAllAsync(
            new GetAllClassRoomsQuery(),
            cancellationToken);

        var buildings = await _buildingQueryService.GetAllAsync(
            new GetAllGymBuildingsQuery(),
            cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid Id, CancellationToken cancellationToken)
    {
        try
        {
            
            await _roomCommandService.DeleteAsync(Id, cancellationToken);
            TempData["SuccessMessage"] = "Room deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to delete room: {ex.Message}";
        }

        return RedirectToPage("./Index");
    }
}