using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassRooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.Rooms;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IClassRoomQueryService _roomQueryService;

    public IndexModel(IClassRoomQueryService roomQueryService)
    {
        _roomQueryService = roomQueryService;
    }

    public IReadOnlyList<ClassRoomDto> Rooms { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Rooms = await _roomQueryService.GetAllAsync(
            new GetAllClassRoomsQuery(),
            cancellationToken);
    }
}