using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;



namespace GymAppV3.Infrastructure.Services
{
    /// <summary>
    /// Service for managing payment transactions and financial reporting
    /// Handles payment recording, VAT calculation, and monthly reports
    /// </summary>
    public class PaymentService : IPaymentService

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

        public async Task<IReadOnlyList<PaymentDto>> GetPaymentsByMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
        {
            
            var payments = await _context.Payments
                .Where(p => p.MemberId == memberId)
                .OrderByDescending(p => p.PaidAt)
                .Select(ObjectMapper.Payment.ToDto)
                .ToListAsync(cancellationToken);

            // Compute derived figures and order in memory.
            return payments;
                
        }

        public async Task<MonthlyFinancialReportDto> GetMonthlyReportAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            var totals = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed
                    && p.PaidAt.Year == year
                    && p.PaidAt.Month == month)
                .GroupBy(_ => 1)
                .Select(g => new {
                                    Count = g.Count(),
                                    Gross = g.Sum(p => p.Amount),
                                    Net = g.Sum(p => p.NetAmount)
                                 })
                .FirstOrDefaultAsync(cancellationToken);

            return new MonthlyFinancialReportDto(
                year, month,
                totals?.Count ?? 0,
                totals?.Gross ?? 0m,
                totals?.Net ?? 0m,
                (totals?.Gross ?? 0m) - (totals?.Net ?? 0m));
        }

        public async Task<PaymentDto> RecordAsync(RecordPaymentCommand command, CancellationToken cancellationToken = default)
        {
            var vatCategory = VatCategory.Services; // default for payments without membership

            // --- The member must exist ---
            var memberExists = await _context.Members
                .AnyAsync(m => m.Id == command.MemberId, cancellationToken);

            if (!memberExists) 
                throw new NotFoundException(nameof(Member), command.MemberId);

            // --- If a membership is referenced, it must exist and belong to this member ---
            if (command.MembershipId is not null)
            {
                var membership = await _context.Memberships
                    .Where(m => m.Id == command.MembershipId && m.MemberId == command.MemberId)
                    .Select(m => new { m.MembershipPackage.VatCategory })
                    .FirstOrDefaultAsync(cancellationToken) ??
                    throw new NotFoundException(nameof(Membership), command.MembershipId);

                vatCategory = membership.VatCategory;
            }

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

            return ToDto(payment);
        }

        private static PaymentDto ToDto(Payment p) => new(
            p.Id, p.MemberId, p.MembershipId,
            p.Amount, p.NetAmount, p.Amount - p.NetAmount, p.VatRate,
            p.Method.ToString(), p.Status.ToString(), p.PaidAt);
    }
}
