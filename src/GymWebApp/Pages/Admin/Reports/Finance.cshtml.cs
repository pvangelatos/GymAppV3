using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Members;
using GymAppV3.Core.Queries.Memberships;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.Reports;

[Authorize(Policy = "AdminOnly")]
public class FinancialModel : PageModel
{
    private readonly IMembershipQueryService _membershipQueryService;
    private readonly IMemberQueryService _memberQueryService;

    public FinancialModel(
        IMembershipQueryService membershipQueryService,
        IMemberQueryService memberQueryService)
    {
        _membershipQueryService = membershipQueryService;
        _memberQueryService = memberQueryService;
    }

    public decimal TotalRevenue { get; private set; }
    public decimal RevenueThisMonth { get; private set; }
    public int ActiveMembersCount { get; private set; }
    public int ActiveMembershipsCount { get; private set; }
    public List<MembershipSaleRow> RecentSales { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        // Fetch all memberships to calculate total revenue and other statistics
        var membersResult = await _memberQueryService.GetAllAsync(
            new GetAllMembersQuery(),
            cancellationToken);

        var allSales = new List<MembershipSaleRow>();
        var activeMembershipsTotal = 0;
        var activeMemberIds = new HashSet<Guid>();

        var now = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        // Fetch all memberships for all members
        foreach (var member in membersResult.Items)
        {
            var memberships = await _membershipQueryService.GetByMemberAsync(
                new GetMembershipsByMemberQuery(member.Id),
                cancellationToken);

            foreach (var m in memberships)
            {
                allSales.Add(new MembershipSaleRow(
                    MemberName: $"{member.Firstname} {member.Lastname}",
                    PackageName: m.PackageName,
                    PurchaseDate: m.StartDate,
                    EndDate: m.EndDate,
                    Price: m.PricePaid
                ));

                if (m.Status == "Active" && m.EndDate > now)
                {
                    activeMembershipsTotal++;
                    activeMemberIds.Add(member.Id);
                }
            }
        }

        // Total revenue and revenue for the current month
        TotalRevenue = allSales.Sum(s => s.Price);
        RevenueThisMonth = allSales.Where(s => s.PurchaseDate >= startOfMonth).Sum(s => s.Price);

        // Active memberships and active members count
        ActiveMembershipsCount = activeMembershipsTotal;
        ActiveMembersCount = activeMemberIds.Count;

        // Recent sales (last 10)
        RecentSales = allSales
            .OrderByDescending(s => s.PurchaseDate)
            .Take(10)
            .ToList();
    }

    public record MembershipSaleRow(
        string MemberName,
        string PackageName,
        DateTimeOffset PurchaseDate,
        DateTimeOffset EndDate,
        decimal Price);
}
