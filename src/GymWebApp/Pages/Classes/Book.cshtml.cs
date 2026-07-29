using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Classes;

public class BookModel : PageModel
{
    public IActionResult OnGet()
    {
        // Booking happens from the class details page after selecting a session
        return RedirectToPage("/Classes/Schedule");
    }
}
