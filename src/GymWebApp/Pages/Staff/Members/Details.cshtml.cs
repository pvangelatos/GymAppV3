using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Staff.Members;

[Authorize(Policy = "StaffOnly")]
public class DetailsModel : PageModel
{
    private readonly IMemberQueryService _memberQueryService;

    public DetailsModel(IMemberQueryService memberQueryService)
    {
        _memberQueryService = memberQueryService;
    }

    public MemberDetailDto? Member { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Member = await _memberQueryService.GetByIdAsync(
            new GetMemberByIdQuery(id),
            cancellationToken);

        if (Member == null)
        {
            return NotFound();
        }

        return Page();
    }
}
