using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Bookings;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Members.Bookings;

[Authorize(Policy = "MemberOnly")]
public class IndexModel : PageModel
{
    private readonly IBookingQueryService _bookingQueryService;
    private readonly IMemberQueryService _memberQueryService;

    public IndexModel(
        IBookingQueryService bookingQueryService,
        IMemberQueryService memberQueryService)
    {
        _bookingQueryService = bookingQueryService;
        _memberQueryService = memberQueryService;
    }

    public ResultSet<BookingDto> Bookings { get; set; } = new([], 0);
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    public async Task<IActionResult> OnGetAsync(int? page, CancellationToken cancellationToken)
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
            TempData["InfoMessage"] = "Please complete your member profile to view bookings.";
            return RedirectToPage("/Members/CompleteProfile");
        }

        CurrentPage = page ?? 1;

        Bookings = await _bookingQueryService.GetByMemberAsync(
            new GetBookingsByMemberQuery(member.Id, new ListOptions { Page = CurrentPage, Size = PageSize }),
            cancellationToken);

        return Page();
    }
}
