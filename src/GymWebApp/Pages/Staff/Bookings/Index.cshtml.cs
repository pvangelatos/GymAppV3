using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Staff.Bookings;

[Authorize(Policy = "StaffOnly")]
public class IndexModel : PageModel
{
    private readonly IMemberQueryService _memberQueryService;

    public IndexModel(IMemberQueryService memberQueryService)
    {
        _memberQueryService = memberQueryService;
    }

    public ResultSet<MemberDto> MembersWithActiveBookings { get; set; } = new([], 0);
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public async Task OnGetAsync(int? page, CancellationToken cancellationToken)
    {
        CurrentPage = page ?? 1;

        MembersWithActiveBookings = await _memberQueryService.GetByActiveBookingsAsync(
            new GetMembersByActiveBookingsQuery(new ListOptions { Page = CurrentPage, Size = PageSize }),
            cancellationToken);
    }
}
