using GymAppV3.Core.Commands;
using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Staff.Members;

[Authorize(Policy = "StaffOnly")]
public class IndexModel : PageModel
{
    private readonly IMemberQueryService _memberQueryService;
    private readonly IMemberCommandService _memberCommandService;

    public IndexModel(
        IMemberQueryService memberQueryService,
        IMemberCommandService memberCommandService)
    {
        _memberQueryService = memberQueryService;
        _memberCommandService = memberCommandService;
    }

    public ResultSet<MemberDto> Members { get; set; } = new([], 0);
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; }

    public async Task OnGetAsync(int pageIndex = 1, CancellationToken cancellationToken = default)
    {
        CurrentPage = pageIndex < 1 ? 1 : pageIndex;


        Members = await _memberQueryService.GetAllAsync(
            new GetAllMembersQuery(
                Options: new ListOptions { Page = CurrentPage, Size = PageSize, Sort = SortBy },
                SearchTerm: SearchTerm),
            cancellationToken);
    }

    // Only the Admin role can delete members (enforced in the domain service too;
    // TrainerAdmin passes the page's StaffOnly/AdminOnly policies but not this specific rule).
    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _memberCommandService.DeleteAsync(new DeleteMemberCommand(id), cancellationToken);
            TempData["SuccessMessage"] = "Member deleted successfully.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (ForbiddenException ex)
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
