using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Core.Queries.Payments;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;




namespace GymAppV3.Infrastructure.Services;

/// <summary>
/// Service for managing payment transactions reporting.
/// Handles payment validation, VAT calculation snapshots, and transaction persistence.
/// </summary>
public class PaymentService : IPaymentCommandService, IPaymentQueryService

{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;
    private readonly IVatRateProvider _vatRates;

    public PaymentService(ApplicationDbContext context, IDateTimeProvider clock, IVatRateProvider vatRates)
    {
        _context = context;
        _clock = clock;
        _vatRates = vatRates;
    }

    public async Task<MonthlyFinancialReportDto> GetMonthlyFinancialReportAsync(GetMonthlyFinancialReportQuery query, CancellationToken cancellationToken = default)
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

    public async Task<IReadOnlyList<PaymentDto>> GetPaymentsByMemberIdAsync(GetPaymentsByMemberQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
           .Where(p => p.MemberId == query.MemberId)
           .OrderByDescending(p => p.PaidAt)
           .Select(ObjectMapper.Payment.ToDto)
           .ToListAsync(cancellationToken);
    }

    public async Task<PaymentDto> RecordAsync(RecordPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var vatCategory = VatCategory.Services; // default for payments without membership

        // --- Member existence check ---
        var memberExists = await _context.Members
            .AnyAsync(m => m.Id == command.MemberId, cancellationToken);

        if (!memberExists) 
            throw new NotFoundException(nameof(Member), command.MemberId);

        // --- Membership verification & VAT category retrieval ---
        if (command.MembershipId is not null)
        {
            var membership = await _context.Memberships
                .Where(m => m.Id == command.MembershipId && m.MemberId == command.MemberId)
                .Select(m => new { m.MembershipPackage.VatCategory })
                .FirstOrDefaultAsync(cancellationToken) ??
                throw new NotFoundException(nameof(Membership), command.MembershipId);

            vatCategory = membership.VatCategory;
        }

        // --- VAT Snapshot Calculation ---
        var rate = _vatRates.GetVatRate(vatCategory);
        var (net, _) = VatCalculator.Split(command.Amount, rate);

        var payment = new Payment
        {
            MemberId = command.MemberId,
            MembershipId = command.MembershipId,
            Amount = command.Amount,
            VatRate = rate,                     // snapshot
            NetAmount = net,                    // snapshot
            Method = command.Method,
            Status = PaymentStatus.Completed,   // recorded payments are completed
            PaidAt = _clock.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return ObjectMapper.Payment.ToDtoCompiled(payment);
    }

}
