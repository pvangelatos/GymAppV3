using GymAppV3.Core.Commands;
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
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Staff.Classes;

[Authorize(Policy = "StaffOnly")]
public class ScheduleModel : PageModel
{
    private readonly IClassSessionCommandService _classSessionCommandService;
    private readonly IClassSessionQueryService _classSessionQueryService;
    private readonly IClassCategoryQueryService _classCategoryQueryService;
    private readonly ITrainerQueryService _trainerQueryService;
    private readonly IClassRoomQueryService _classRoomQueryService;

    public ScheduleModel(
        IClassSessionCommandService classSessionCommandService,
        IClassSessionQueryService classSessionQueryService,
        IClassCategoryQueryService classCategoryQueryService,
        ITrainerQueryService trainerQueryService,
        IClassRoomQueryService classRoomQueryService)
    {
        _classSessionCommandService = classSessionCommandService;
        _classSessionQueryService = classSessionQueryService;
        _classCategoryQueryService = classCategoryQueryService;
        _trainerQueryService = trainerQueryService;
        _classRoomQueryService = classRoomQueryService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Categories { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
    public SelectList Trainers { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
    public SelectList Rooms { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

    public class InputModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public Guid ClassCategoryId { get; set; }

        [Required]
        public DateTime StartsAt { get; set; } = DateTime.Now.AddDays(1);

        [Required]
        [Range(15, 180)]
        public int DurationInMinutes { get; set; } = 50;

        [Required]
        [Range(1, 50)]
        public int Capacity { get; set; } = 20;

        [Required]
        public Guid TrainerId { get; set; }

        [Required]
        public Guid ClassRoomId { get; set; }
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadDropdownsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnGetEventsAsync(DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        var sessions = await _classSessionQueryService.GetUpcomingAsync(
            new GetUpcomingClassSessionsQuery(start.ToUniversalTime(), end.ToUniversalTime()),
            cancellationToken);

        var events = sessions.Select(s => new
        {
            title = $"{s.Title} ({s.TrainerName} · {s.ClassRoomName})",
            start = s.StartsAt.ToString("o"),
            end = s.StartsAt.AddMinutes(s.DurationInMinutes).ToString("o"),
            color = s.AvailableSeats > 5 ? "#8FB577" : s.AvailableSeats > 0 ? "#D9A54B" : "#B85450"
        });

        return new JsonResult(events);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(cancellationToken);
            return Page();
        }

        try
        {
            var command = new ScheduleClassSessionCommand(
                Title: Input.Title,
                ClassCategoryId: Input.ClassCategoryId,
                StartsAt: Input.StartsAt.ToUniversalTime(),
                DurationInMinutes: Input.DurationInMinutes,
                Capacity: Input.Capacity,
                TrainerId: Input.TrainerId,
                ClassRoomId: Input.ClassRoomId
            );

            await _classSessionCommandService.ScheduleAsync(command, cancellationToken);

            TempData["SuccessMessage"] = "Class session scheduled successfully!";
            return RedirectToPage("/Staff/Classes/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            await LoadDropdownsAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadDropdownsAsync(CancellationToken cancellationToken)
    {
        var categories = await _classCategoryQueryService.GetAllAsync(
            new GetAllClassCategoriesQuery(), cancellationToken);

        var trainers = await _trainerQueryService.GetAllAsync(
            new GetAllTrainersQuery(), cancellationToken);

        var rooms = await _classRoomQueryService.GetAllAsync(
            new GetAllClassRoomsQuery(), cancellationToken);

        Categories = new SelectList(categories, nameof(ClassCategoryDto.Id), nameof(ClassCategoryDto.Name));

        // ✅ FIX: TrainerDto has no "Name" — project to anonymous type first
        var trainerItems = trainers.Items.Select(t => new
        {
            t.Id,
            Name = $"{t.Firstname} {t.Lastname}"
        });
        Trainers = new SelectList(trainerItems, "Id", "Name");

        Rooms = new SelectList(rooms, nameof(ClassRoomDto.Id), nameof(ClassRoomDto.ClassRoomName));
    }
}
