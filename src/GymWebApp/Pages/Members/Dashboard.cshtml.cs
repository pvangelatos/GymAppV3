using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Bookings;
using GymAppV3.Core.Queries.Members;
using GymAppV3.Core.Queries.Memberships;
using GymAppV3.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Members;

[Authorize(Policy = "MemberOnly")]
public class DashboardModel : PageModel
{
    private readonly IMemberQueryService _memberQueryService;
    private readonly IBookingQueryService _bookingQueryService;
    private readonly IMembershipQueryService _membershipQueryService;

    public DashboardModel(
        IMemberQueryService memberQueryService,
        IBookingQueryService bookingQueryService,
        IMembershipQueryService membershipQueryService)
    {
        _memberQueryService = memberQueryService;
        _bookingQueryService = bookingQueryService;
        _membershipQueryService = membershipQueryService;
    }

    public MemberDto? Member { get; set; }
    public IReadOnlyList<BookingDto> ActiveBookings { get; set; } = [];
    public IReadOnlyList<MembershipDto> ActiveMemberships { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        // Get member profile
        Member = await _memberQueryService.GetByUserIdAsync(
            new GetMemberByUserIdQuery(userId),
            cancellationToken);

        if (Member == null)
        {
            TempData["InfoMessage"] = "Please complete your member profile to access all features.";
            return RedirectToPage("/Members/CompleteProfile");
        }

        // Get active bookings
        var bookingsResult = await _bookingQueryService.GetByMemberAsync(
            new GetBookingsByMemberQuery(Member.Id, new ListOptions { Page = 1, Size = 10 }),
            cancellationToken);
        ActiveBookings = bookingsResult.Items
            .Where(b => b.Status == "Confirmed" && b.SessionStartsAt > DateTimeOffset.UtcNow)
            .ToList();

        // Get active memberships
        var memberships = await _membershipQueryService.GetByMemberAsync(
            new GetMembershipsByMemberQuery(Member.Id),
            cancellationToken);
        ActiveMemberships = memberships
            .Where(m => m.Status == "Active" && m.EndDate > DateTimeOffset.UtcNow)
            .ToList();

        return Page();
    }
}
