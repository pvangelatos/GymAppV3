using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Trainers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.Trainers;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ITrainerQueryService _trainerQueryService;

    public IndexModel(ITrainerQueryService trainerQueryService)
    {
        _trainerQueryService = trainerQueryService;
    }

    public IReadOnlyList<TrainerDto> Trainers { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Trainers = await _trainerQueryService.GetAllAsync(
            new GetAllTrainersQuery(),
            cancellationToken);
    }
}
