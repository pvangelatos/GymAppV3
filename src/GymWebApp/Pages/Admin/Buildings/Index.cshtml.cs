using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.GymBuildings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.Buildings;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IGymBuildingQueryService _buildingQueryService;

    public IndexModel(IGymBuildingQueryService buildingQueryService)
    {
        _buildingQueryService = buildingQueryService;
    }

    public IReadOnlyList<GymBuildingDto> Buildings { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Buildings = await _buildingQueryService.GetAllAsync(
            new GetAllGymBuildingsQuery(),
            cancellationToken);
    }
}