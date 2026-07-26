using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassCategories;
using GymAppV3.Core.Queries.ClassSessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
}
