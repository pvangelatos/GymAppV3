using GymAppV3.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Members.Bookings;

[Authorize(Policy = "MemberOnly")]
public class CancelModel : PageModel
{
    private readonly IBookingCommandService _bookingCommandService;

    public CancelModel(IBookingCommandService bookingCommandService)
    {
        _bookingCommandService = bookingCommandService;
    }

    [BindProperty]
    public Guid BookingId { get; set; }

    public IActionResult OnGet(Guid id)
    {
        BookingId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _bookingCommandService.CancelAsync(BookingId, cancellationToken);

            TempData["SuccessMessage"] = "Booking cancelled successfully.";
            return RedirectToPage("/Members/Bookings/Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to cancel booking: {ex.Message}";
            return RedirectToPage("/Members/Bookings/Index");
        }
    }
}
