using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Members;

[Authorize(Policy = "MemberOnly")]
public class ProfileModel : PageModel
{
    private readonly IMemberQueryService _memberQueryService;

    public ProfileModel(IMemberQueryService memberQueryService)
    {
        _memberQueryService = memberQueryService;
    }

    public MemberDetailDto? Member { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var memberDto = await _memberQueryService.GetByUserIdAsync(
            new GetMemberByUserIdQuery(userId),
            cancellationToken);

        if (memberDto == null)
        {
            TempData["InfoMessage"] = "Please complete your member profile.";
            return RedirectToPage("/Members/CompleteProfile");
        }

        // Get the detailed version
        Member = await _memberQueryService.GetByIdAsync(
            new GetMemberByIdQuery(memberDto.Id),
            cancellationToken);

        return Page();
    }
}
