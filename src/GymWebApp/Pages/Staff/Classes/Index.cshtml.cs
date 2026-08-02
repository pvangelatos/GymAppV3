using GymAppV3.Core.Command;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassSessions;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace GymWebApp.Pages.Staff.Classes;

[Authorize(Policy = "StaffOnly")]
public class IndexModel : PageModel
{
    private readonly IClassSessionQueryService _classSessionQueryService;
    private readonly IMemberQueryService _memberQueryService;
    private readonly IBookingCommandService _bookingCommandService;
    private readonly IClassSessionCommandService _classSessionCommandService;

    public IndexModel(
        IClassSessionQueryService classSessionQueryService,
        IMemberQueryService memberQueryService,
        IBookingCommandService bookingCommandService,
        IClassSessionCommandService classSessionCommandService  )
    {
        _classSessionQueryService = classSessionQueryService;
        _memberQueryService = memberQueryService;
        _bookingCommandService = bookingCommandService;
        _classSessionCommandService = classSessionCommandService;
    }

    public void OnGet()
    {
        // calendar loads events from OnGetEventsAsync
    }

    public async Task<IActionResult> OnGetEventsAsync(DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        var sessions = await _classSessionQueryService.GetUpcomingAsync(
            new GetUpcomingClassSessionsQuery(start.ToUniversalTime(), end.ToUniversalTime()),
            cancellationToken);

        var events = sessions.Select(s => new
        {
            id = s.Id,
            title = $"{s.ClassCategoryName} · {s.TrainerName} ({s.AvailableSeats}/{s.Capacity})",
            start = s.StartsAt.ToString("o"),
            end = s.StartsAt.AddMinutes(s.DurationInMinutes).ToString("o"),
            color = s.AvailableSeats > 5 ? "#8FB577" : s.AvailableSeats > 0 ? "#D9A54B" : "#B85450",
            extendedProps = new
            {
                classCategoryId = s.ClassCategoryId,
                trainerName = s.TrainerName,
                roomName = s.ClassRoomName,
                capacity = s.Capacity,
                availableSeats = s.AvailableSeats
            }
        });

        return new JsonResult(events, CamelCaseJson);
    }

    public async Task<IActionResult> OnGetBookingCandidatesAsync(Guid classCategoryId, string? term, CancellationToken cancellationToken)
    {
        var candidates = await _memberQueryService.GetBookingCandidatesAsync(
            new GetBookingCandidatesQuery(classCategoryId, term),
            cancellationToken);

        return new JsonResult(candidates, CamelCaseJson);
    }

    public async Task<IActionResult> OnPostAddBookingAsync(Guid memberId, Guid classSessionId, CancellationToken cancellationToken)
    {
        try
        {
            await _bookingCommandService.BookAsync(new CreateBookingCommand(memberId, classSessionId), cancellationToken);
            return new JsonResult(new { success = true }, CamelCaseJson);
        }
        catch (BusinessRuleException ex)
        {
            return new JsonResult(new { success = false, message = ex.Message }, CamelCaseJson);
        }
        catch (NotFoundException ex)
        {
            return new JsonResult(new { success = false, message = ex.Message }, CamelCaseJson);
        }
    }

    public async Task<IActionResult> OnPostDuplicateWeekAsync(DateTime weekStart, DateTime weekEnd, int repeatWeeks, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _classSessionCommandService.DuplicateWeekAsync(
                new DuplicateWeekCommand(weekStart.ToUniversalTime(), weekEnd.ToUniversalTime(), repeatWeeks),
                cancellationToken);

            return new JsonResult(new { success = true, count = created.Count }, CamelCaseJson);
        }
        catch (BusinessRuleException ex)
        {
            return new JsonResult(new { success = false, message = ex.Message }, CamelCaseJson);
        }
    }

    private static readonly JsonSerializerOptions CamelCaseJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };


}

