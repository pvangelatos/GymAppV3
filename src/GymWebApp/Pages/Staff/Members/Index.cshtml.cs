using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Staff.Members;

[Authorize(Policy = "StaffOnly")]
public class IndexModel : PageModel
{
    private readonly IMemberQueryService _memberQueryService;

    public IndexModel(IMemberQueryService memberQueryService)
    {
        _memberQueryService = memberQueryService;
    }

    public ResultSet<MemberDto> Members { get; set; } = new([], 0);
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public async Task OnGetAsync(int? page, CancellationToken cancellationToken)
    {
        CurrentPage = page ?? 1;

        Members = await _memberQueryService.GetAllAsync(
            new GetAllMembersQuery(new ListOptions { Page = CurrentPage, Size = PageSize }),
            cancellationToken);
    }
}
