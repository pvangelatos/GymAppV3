using GymAppV3.Core.Commands;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassRooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Admin.Rooms;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly IClassRoomQueryService _roomQueryService;
    private readonly IClassRoomCommandService _roomCommandService;

    public EditModel(
        IClassRoomQueryService roomQueryService,
        IClassRoomCommandService roomCommandService)
    {
        _roomQueryService = roomQueryService;
        _roomCommandService = roomCommandService;
    }

    [BindProperty]
    public Guid RoomId { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string BuildingName { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [Display(Name = "Room Name")]
        [StringLength(100)]
        public string ClassRoomName { get; set; } = string.Empty;

        [Required]
        [Range(1, 500)]
        public int Capacity { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        RoomId = id;

        var room = await _roomQueryService.GetByIdAsync(
            new GetClassRoomByIdQuery(id),
            cancellationToken);

        if (room == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            ClassRoomName = room.ClassRoomName,
            Capacity = room.Capacity
        };

        BuildingName = $"Building ID: {room.GymBuildingId}";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var command = new UpdateClassRoomCommand(
            Input.ClassRoomName,
            Input.Capacity);

        await _roomCommandService.UpdateAsync(RoomId, command, cancellationToken);

        TempData["SuccessMessage"] = $"Room '{Input.ClassRoomName}' updated successfully.";
        return RedirectToPage("./Index");
    }
}
