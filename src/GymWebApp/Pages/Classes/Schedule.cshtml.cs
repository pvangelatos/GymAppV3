using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassCategories;
using GymAppV3.Core.Queries.ClassRooms;
using GymAppV3.Core.Queries.ClassSessions;
using GymAppV3.Core.Queries.Trainers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymWebApp.Pages.Classes;

[Authorize(Policy = "MemberOnly")]
public class ScheduleModel : PageModel
{
    private readonly IClassSessionQueryService _classSessionQueryService;
    private readonly IClassCategoryQueryService _classCategoryQueryService;

    public ScheduleModel(
        IClassSessionQueryService classSessionQueryService,
        IClassCategoryQueryService classCategoryQueryService)
    {
        _classSessionQueryService = classSessionQueryService;
        _classCategoryQueryService = classCategoryQueryService;
    }

    public IReadOnlyList<ClassSessionDto> ClassSessions { get; set; } = [];
    public IReadOnlyList<ClassCategoryDto> Categories { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public Guid? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        // Get all categories for the filter dropdown
        Categories = await _classCategoryQueryService.GetAllAsync(
            new GetAllClassCategoriesQuery(),
            cancellationToken);

        // Set default date range if not specified
        var from = FromDate?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
        var to = ToDate?.ToUniversalTime() ?? DateTimeOffset.UtcNow.AddDays(14);

        // Get upcoming class sessions
        var allSessions = await _classSessionQueryService.GetUpcomingAsync(
            new GetUpcomingClassSessionsQuery(from, to),
            cancellationToken);

        // Filter by category if specified
        ClassSessions = CategoryId.HasValue
            ? allSessions.Where(s => s.ClassCategoryId == CategoryId.Value).ToList()
            : allSessions;
    }

    public async Task<IActionResult> OnGetEventsAsync(DateTime start, DateTime end, Guid? categoryId, CancellationToken cancellationToken)
    {
        var sessions = await _classSessionQueryService.GetUpcomingAsync(
            new GetUpcomingClassSessionsQuery(start.ToUniversalTime(), end.ToUniversalTime()),
            cancellationToken);

        if (categoryId.HasValue)
        {
            sessions = sessions.Where(s => s.ClassCategoryId == categoryId.Value).ToList();
        }

        var events = sessions.Select(s => new
        {
            title = s.Title,
            start = s.StartsAt.ToString("o"),
            end = s.StartsAt.AddMinutes(s.DurationInMinutes).ToString("o"),
            color = s.AvailableSeats > 5 ? "#8FB577" : s.AvailableSeats > 0 ? "#D9A54B" : "#B85450",
            url = Url.Page("/Classes/Details", new { id = s.Id })
        });

        return new JsonResult(events);
    }
}
