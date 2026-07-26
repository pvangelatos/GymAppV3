using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassSessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Staff.Classes;

[Authorize(Policy = "StaffOnly")]
public class DetailsModel : PageModel
{
    private readonly IClassSessionQueryService _classSessionQueryService;

    public DetailsModel(IClassSessionQueryService classSessionQueryService)
    {
        _classSessionQueryService = classSessionQueryService;
    }

    public ClassSessionDto? ClassSession { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        ClassSession = await _classSessionQueryService.GetClassSessionByIdAsync(
            new GetClassSessionByIdQuery(id),
            cancellationToken);

        if (ClassSession == null)
        {
            return NotFound();
        }

        return Page();
    }
}
