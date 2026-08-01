using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
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
    private readonly ITrainerCommandService _trainerCommandService;

    public IndexModel(
        ITrainerQueryService trainerQueryService,
        ITrainerCommandService trainerCommandService)
    {
        _trainerQueryService = trainerQueryService;
        _trainerCommandService = trainerCommandService;
    }

    public ResultSet<TrainerDto> Trainers { get; set; } = new([], 0);
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Trainers = await _trainerQueryService.GetAllAsync(
            new GetAllTrainersQuery(new ListOptions(CurrentPage, PageSize)),
            cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _trainerCommandService.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Trainer deleted successfully.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage("./Index");
    }
}
