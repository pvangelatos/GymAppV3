using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.MembershipPackages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.MembershipPackages;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IMembershipPackageQueryService _packageQueryService;
    private readonly IMembershipPackageCommandService _packageCommandService;

    public IndexModel(IMembershipPackageQueryService packageQueryService, IMembershipPackageCommandService packageCommandService)
    {
        _packageQueryService = packageQueryService;
        _packageCommandService = packageCommandService;
    }

    public IReadOnlyList<MembershipPackageDto> Packages { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Packages = await _packageQueryService.GetAllAsync(
            new GetAllMembershipPackagesQuery(),
            cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _packageCommandService.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Membership package deleted successfully.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage("./Index");
    }
}