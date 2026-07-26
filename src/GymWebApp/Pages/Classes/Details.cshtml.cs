using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassSessions;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Classes;

[Authorize(Policy = "MemberOnly")]
public class DetailsModel : PageModel
{
    private readonly IClassSessionQueryService _classSessionQueryService;
    private readonly IMemberQueryService _memberQueryService;
    private readonly IBookingCommandService _bookingCommandService;

    public DetailsModel(
        IClassSessionQueryService classSessionQueryService,
        IMemberQueryService memberQueryService,
        IBookingCommandService bookingCommandService)
    {
        _classSessionQueryService = classSessionQueryService;
        _memberQueryService = memberQueryService;
        _bookingCommandService = bookingCommandService;
    }

    public ClassSessionDto? ClassSession { get; set; }
    public bool CanBook { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        ClassSession = await _classSessionQueryService.GetClassSessionByIdAsync(
            new GetClassSessionByIdQuery(id),
            cancellationToken);

        if (ClassSession == null)
        {
            return NotFound();
        }

        // Check if user has a member profile
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            var member = await _memberQueryService.GetByUserIdAsync(
                new GetMemberByUserIdQuery(userId),
                cancellationToken);

            CanBook = member != null && ClassSession.AvailableSeats > 0;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostBookAsync(Guid id, CancellationToken cancellationToken)
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
            TempData["ErrorMessage"] = "Please complete your member profile before booking classes.";
            return RedirectToPage("/Members/CompleteProfile");
        }

        try
        {
            var command = new CreateBookingCommand(
                MemberId: member.Id,
                ClassSessionId: id
            );

            await _bookingCommandService.BookAsync(command, cancellationToken);

            TempData["SuccessMessage"] = "Class booked successfully!";
            return RedirectToPage("/Members/Bookings/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await OnGetAsync(id, cancellationToken);
            return Page();
        }
    }
}
