using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Classes;

[Authorize(Policy = "MemberOnly")]
public class BookModel : PageModel
{
    public IActionResult OnGet()
    {
        // Redirect to schedule page where users can browse and book classes
        return RedirectToPage("/Classes/Schedule");
    }
}
