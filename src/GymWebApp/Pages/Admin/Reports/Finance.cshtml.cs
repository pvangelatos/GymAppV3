using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.Reports;

[Authorize(Policy = "AdminOnly")]
public class FinancialModel : PageModel
{
    // TODO: inject IMembershipQueryService, IMemberQueryService when report queries are available

    public decimal TotalRevenue { get; private set; }
    public decimal RevenueThisMonth { get; private set; }
    public int ActiveMembersCount { get; private set; }
    public int ActiveMembershipsCount { get; private set; }
    public List<MembershipSaleRow> RecentSales { get; private set; } = [];

    public void OnGet()
    {
        // Placeholder — wire up real query services when financial queries are implemented
    }

    public record MembershipSaleRow(
        string MemberName,
        string PackageName,
        DateTimeOffset PurchaseDate,
        DateTimeOffset EndDate,
        decimal Price);
}
