using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.MembershipPackages;
using GymAppV3.Core.Queries.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymWebApp.Pages.Staff.Memberships;

[Authorize(Policy = "StaffOnly")]
public class CreateModel : PageModel
{
    private readonly IMembershipPackageQueryService _packageQueryService;
    private readonly IMemberQueryService _memberQueryService;
    private readonly IMembershipCommandService _membershipCommandService;

    public CreateModel(
        IMembershipPackageQueryService packageQueryService,
        IMemberQueryService memberQueryService,
        IMembershipCommandService membershipCommandService)
    {
        _packageQueryService = packageQueryService;
        _memberQueryService = memberQueryService;
        _membershipCommandService = membershipCommandService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid MemberId { get; set; }

    [BindProperty]
    public Guid MembershipPackageId { get; set; }

    public MemberDetailDto? Member { get; set; }
    public SelectList Packages { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Member = await _memberQueryService.GetByIdAsync(new GetMemberByIdQuery(MemberId), cancellationToken);
        if (Member == null)
        {
            return NotFound();
        }

        await LoadPackagesAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _membershipCommandService.PurchaseAsync(
                new PurchaseMembershipCommand(MemberId, MembershipPackageId),
                cancellationToken);

            TempData["SuccessMessage"] = "Membership assigned successfully.";
            return RedirectToPage("/Staff/Members/Details", new { id = MemberId });
        }
        catch (BusinessRuleException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (NotFoundException ex)
        {
            ErrorMessage = ex.Message;
        }

        Member = await _memberQueryService.GetByIdAsync(new GetMemberByIdQuery(MemberId), cancellationToken);
        await LoadPackagesAsync(cancellationToken);
        return Page();
    }

    private async Task LoadPackagesAsync(CancellationToken cancellationToken)
    {
        var packages = await _packageQueryService.GetAllAsync(new GetAllMembershipPackagesQuery(), cancellationToken);
        Packages = new SelectList(packages, nameof(MembershipPackageDto.Id), nameof(MembershipPackageDto.Name));
    }
}
