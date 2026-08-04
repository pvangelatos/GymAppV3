using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.ClassSessions;
using GymAppV3.Core.Queries.Members;
using GymAppV3.Core.Queries.Memberships;
using GymAppV3.Core.Queries.Payments;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymWebApp.Pages.Admin.Reports;

[Authorize(Policy = "AdminOnly")]
public class FinancialModel : PageModel
{
    private readonly IMembershipQueryService _membershipQueryService;
    private readonly IMemberQueryService _memberQueryService;
    private readonly IPaymentQueryService _paymentQueryService;
    private readonly IClassSessionQueryService _classSessionQueryService;

    public FinancialModel(
        IMembershipQueryService membershipQueryService,
        IMemberQueryService memberQueryService,
        IPaymentQueryService paymentQueryService,
        IClassSessionQueryService classSessionQueryService)
    {
        _membershipQueryService = membershipQueryService;
        _memberQueryService = memberQueryService;
        _paymentQueryService = paymentQueryService;
        _classSessionQueryService = classSessionQueryService;
    }

    public decimal TotalRevenue { get; private set; }
    public decimal RevenueThisMonth { get; private set; }
    public int ActiveMembersCount { get; private set; }
    public int ActiveMembershipsCount { get; private set; }
    public List<MembershipSaleRow> RecentSales { get; private set; } = [];

    public List<string> RevenueMonthLabels { get; private set; } = [];
    public List<decimal> RevenueMonthValues { get; private set; } = [];
    public List<string> CategoryLabels { get; private set; } = [];
    public List<double> CategoryUtilization { get; private set; } = [];

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

        // --- Revenue trend, last 6 months (uses the VAT-aware financial report) ---
        var nowUtc = DateTime.UtcNow;
        for (var i = 5; i >= 0; i--)
        {
            var month = nowUtc.AddMonths(-i);
            var report = await _paymentQueryService.GetMonthlyFinancialReportAsync(
                new GetMonthlyFinancialReportQuery(month.Year, month.Month),
                cancellationToken);

            RevenueMonthLabels.Add(month.ToString("MMM yyyy"));
            RevenueMonthValues.Add(report.TotalGross);
        }

        // --- Utilization by Class Category (upcoming sessions, next 30 days) ---
        var upcoming = await _classSessionQueryService.GetUpcomingAsync(
            new GetUpcomingClassSessionsQuery(nowUtc, nowUtc.AddDays(30)),
            cancellationToken);

        var byCategory = upcoming
            .GroupBy(s => s.ClassCategoryName)
            .Select(g => new
            {
                Category = g.Key,
                Utilization = g.Average(s => s.Capacity == 0 ? 0 : (double)(s.Capacity - s.AvailableSeats) / s.Capacity * 100)
            })
            .OrderByDescending(x => x.Utilization)
            .ToList();

        CategoryLabels = byCategory.Select(x => x.Category).ToList();
        CategoryUtilization = byCategory.Select(x => Math.Round(x.Utilization, 1)).ToList();
    }

    public record MembershipSaleRow(
        string MemberName,
        string PackageName,
        DateTimeOffset PurchaseDate,
        DateTimeOffset EndDate,
        decimal Price);
}