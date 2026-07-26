using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.MembershipPackages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.MembershipPackages;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IMembershipPackageQueryService _packageQueryService;

    public IndexModel(IMembershipPackageQueryService packageQueryService)
    {
        _packageQueryService = packageQueryService;
    }

    public IReadOnlyList<MembershipPackageDto> Packages { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Packages = await _packageQueryService.GetAllAsync(
            new GetAllMembershipPackagesQuery(),
            cancellationToken);
    }
}