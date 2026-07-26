using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Members;
using GymAppV3.Core.Queries.Memberships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Members.Memberships;

[Authorize(Policy = "MemberOnly")]
public class IndexModel : PageModel
{
    private readonly IMembershipQueryService _membershipQueryService;
    private readonly IMemberQueryService _memberQueryService;

    public IndexModel(
        IMembershipQueryService membershipQueryService,
        IMemberQueryService memberQueryService)
    {
        _membershipQueryService = membershipQueryService;
        _memberQueryService = memberQueryService;
    }

    public IReadOnlyList<MembershipDto> Memberships { get; set; } = [];
    public IReadOnlyList<MembershipDto> ActiveMemberships { get; set; } = [];
    public IReadOnlyList<MembershipDto> ExpiredMemberships { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
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
            TempData["InfoMessage"] = "Please complete your member profile to view memberships.";
            return RedirectToPage("/Members/CompleteProfile");
        }

        Memberships = await _membershipQueryService.GetByMemberAsync(
            new GetMembershipsByMemberQuery(member.Id),
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        ActiveMemberships = Memberships
            .Where(m => m.Status == "Active" && m.EndDate > now)
            .OrderByDescending(m => m.EndDate)
            .ToList();

        ExpiredMemberships = Memberships
            .Where(m => m.Status == "Expired" || m.EndDate <= now)
            .OrderByDescending(m => m.EndDate)
            .ToList();

        return Page();
    }
}
