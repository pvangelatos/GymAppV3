using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.GymBuildings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.Buildings;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IGymBuildingQueryService _buildingQueryService;
    private readonly IGymBuildingCommandService _buildingCommandService;

    public IndexModel(
        IGymBuildingQueryService buildingQueryService,
        IGymBuildingCommandService buildingCommandService)
    {
        _buildingQueryService = buildingQueryService;
        _buildingCommandService = buildingCommandService;
    }

    public IReadOnlyList<GymBuildingDto> Buildings { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Buildings = await _buildingQueryService.GetAllAsync(
            new GetAllGymBuildingsQuery(),
            cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _buildingCommandService.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Building deleted successfully.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage("./Index");
    }
}