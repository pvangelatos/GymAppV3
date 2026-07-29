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
using System.Security.Claims;

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
        // Retrieve the user ID from the claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Fetch the member details using the user ID
        Member = await _memberQueryService.GetByUserIdAsync(
            new GetMemberByUserIdQuery(userId),
            cancellationToken);

        // If the member profile is not found, redirect to the profile completion page
        if (Member is null) 
        {
            TempData["InfoMessage"] = "Please complete your member profile to access all features.";
            return RedirectToPage("/Members/CompleteProfile");
        }

        // Fetch active bookings for the member
        var bookingsResult = await _bookingQueryService.GetByMemberAsync(
            new GetBookingsByMemberQuery(
                Member.Id,
                new ListOptions { Page = 1, Size = 10 },
                OnlyActive: true),
            cancellationToken);

        // Assign the active bookings to the property
        ActiveBookings = bookingsResult.Items;

        // Fetch active memberships for the member
        ActiveMemberships = await _membershipQueryService.GetByMemberAsync(
            new GetMembershipsByMemberQuery(
                Member.Id,
                OnlyActive: true),
            cancellationToken);

        // Return the page with the populated data
        return Page();
    }
}
