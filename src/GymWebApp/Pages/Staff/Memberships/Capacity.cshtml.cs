using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Staff.Memberships;

[Authorize(Policy = "StaffOnly")]
public class CapacityModel : PageModel
{
    private readonly IMembershipPackageQueryService _packageQueryService;

    public CapacityModel(IMembershipPackageQueryService packageQueryService)
    {
        _packageQueryService = packageQueryService;
    }

    public IReadOnlyList<MembershipPackageAvailabilityDto> Packages { get; set; } = Array.Empty<MembershipPackageAvailabilityDto>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Packages = await _packageQueryService.GetAvailabilityAsync(cancellationToken);
    }
}
