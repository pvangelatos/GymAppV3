using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Queries.Payments;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


public class GetMonthlyFinancialReportQueryHandler
    : IQueryHandler<GetMonthlyFinancialReportQuery, MonthlyFinancialReportDto>
{
    private readonly ApplicationDbContext _context;

    public GetMonthlyFinancialReportQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MonthlyFinancialReportDto> HandleAsync(
        GetMonthlyFinancialReportQuery query,
        CancellationToken cancellationToken = default)
    {
        // Validation for valid month and date
        if (query.Year < 2000 || query.Month < 1 || query.Month > 12)
            throw new BusinessRuleException("Invalid year or month provided for financial report.");

        // limits for dates for index-friendly SQL query
        var startOfMonth = new DateTimeOffset(query.Year, query.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var endOfMonth = startOfMonth.AddMonths(1);

        var totals = await _context.Payments
                   .Where(p => p.Status == PaymentStatus.Completed
                       && p.PaidAt >= startOfMonth
                       && p.PaidAt < endOfMonth)
                   .GroupBy(_ => 1)
                   .Select(g => new
                   {
                       Count = g.Count(),
                       Gross = g.Sum(p => p.Amount),
                       Net = g.Sum(p => p.NetAmount)
                   })
                   .FirstOrDefaultAsync(cancellationToken);

        return new MonthlyFinancialReportDto(
            query.Year, query.Month,
            totals?.Count ?? 0,
            totals?.Gross ?? 0m,
            totals?.Net ?? 0m,
            (totals?.Gross ?? 0m) - (totals?.Net ?? 0m));
    }
}

