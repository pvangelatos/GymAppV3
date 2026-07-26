using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.MembershipPackages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Memberships;

[Authorize(Policy = "MemberOnly")]
public class PackagesModel : PageModel
{
    private readonly IMembershipPackageQueryService _membershipPackageQueryService;

    public PackagesModel(IMembershipPackageQueryService membershipPackageQueryService)
    {
        _membershipPackageQueryService = membershipPackageQueryService;
    }

    public IReadOnlyList<MembershipPackageDto> Packages { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Packages = await _membershipPackageQueryService.GetAllAsync(
            new GetAllMembershipPackagesQuery(),
            cancellationToken);
    }
}
