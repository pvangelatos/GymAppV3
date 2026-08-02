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

    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; }



    public async Task OnGetAsync(int pageIndex = 1, CancellationToken cancellationToken = default)
    {
        CurrentPage = pageIndex < 1 ? 1 : pageIndex;

        Trainers = await _trainerQueryService.GetAllAsync(
            new GetAllTrainersQuery(new ListOptions{ Page = CurrentPage, Size = PageSize, Sort = SortBy }),
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

    public string SortRoute(string column) =>
        !string.IsNullOrEmpty(SortBy) && SortBy.Equals(column, StringComparison.OrdinalIgnoreCase)
        ? $"{column} desc" : column;

    public string SortIcon(string column)
    {
        if (string.IsNullOrEmpty(SortBy)) return "bi-arrow-down-up text-muted";
        if (SortBy.Equals(column, StringComparison.OrdinalIgnoreCase)) return "bi-sort-up-alt";
        if (SortBy.Equals($"{column} desc", StringComparison.OrdinalIgnoreCase)) return "bi-sort-down-alt";
        return "bi-arrow-down-up text-muted";
    }
}
