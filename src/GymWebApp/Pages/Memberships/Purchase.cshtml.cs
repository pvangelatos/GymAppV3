using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.MembershipPackages;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Memberships;

[Authorize(Policy = "MemberOnly")]
public class PurchaseModel : PageModel
{
    private readonly IMembershipPackageQueryService _membershipPackageQueryService;
    private readonly IMemberQueryService _memberQueryService;
    private readonly IMembershipCommandService _membershipCommandService;

    public PurchaseModel(
        IMembershipPackageQueryService membershipPackageQueryService,
        IMemberQueryService memberQueryService,
        IMembershipCommandService membershipCommandService)
    {
        _membershipPackageQueryService = membershipPackageQueryService;
        _memberQueryService = memberQueryService;
        _membershipCommandService = membershipCommandService;
    }

    public MembershipPackageDto? Package { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty]
    public Guid PackageId { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Package = await _membershipPackageQueryService.GetByIdAsync(
            new GetMembershipPackageByIdQuery(id),
            cancellationToken);

        if (Package == null)
        {
            return NotFound();
        }

        PackageId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var member = await _memberQueryService.GetByUserIdAsync(
            new GetMemberByUserIdQuery(userId),
            cancellationToken);

        if (member == null)
        {
            TempData["ErrorMessage"] = "Please complete your member profile before purchasing a membership.";
            return RedirectToPage("/Members/CompleteProfile");
        }

        try
        {
            var command = new PurchaseMembershipCommand(
                MemberId: member.Id,
                MembershipPackageId: PackageId
            );

            await _membershipCommandService.PurchaseAsync(command, cancellationToken);

            TempData["SuccessMessage"] = "Membership purchased successfully!";
            return RedirectToPage("/Members/Memberships/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await OnGetAsync(PackageId, cancellationToken);
            return Page();
        }
    }
}
