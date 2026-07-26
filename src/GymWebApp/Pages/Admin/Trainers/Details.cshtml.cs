using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Trainers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.Trainers;

[Authorize(Policy = "AdminOnly")]
public class DetailsModel : PageModel
{
    private readonly ITrainerQueryService _trainerQueryService;

    public DetailsModel(ITrainerQueryService trainerQueryService)
    {
        _trainerQueryService = trainerQueryService;
    }

    public TrainerDto Trainer { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var trainer = await _trainerQueryService.GetByIdAsync(
            new GetTrainerByIdQuery(id),
            cancellationToken);

        if (trainer == null)
        {
            return NotFound();
        }

        Trainer = trainer;
        return Page();
    }
}
