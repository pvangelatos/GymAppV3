using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassSessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Staff.Classes;

[Authorize(Policy = "StaffOnly")]
public class IndexModel : PageModel
{
    private readonly IClassSessionQueryService _classSessionQueryService;

    public IndexModel(IClassSessionQueryService classSessionQueryService)
    {
        _classSessionQueryService = classSessionQueryService;
    }

    public IReadOnlyList<ClassSessionDto> ClassSessions { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var from = FromDate?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
        var to = ToDate?.ToUniversalTime() ?? DateTimeOffset.UtcNow.AddDays(30);

        ClassSessions = await _classSessionQueryService.GetUpcomingAsync(
            new GetUpcomingClassSessionsQuery(from, to),
            cancellationToken);
    }
}
