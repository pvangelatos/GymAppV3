using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs.Payment;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Infrastructure.Services
{
    public class PaymentService : IPaymentService

    {
        public readonly ApplicationDbContext _context;
        private readonly IDateTimeProvider _clock;

        public PaymentService(ApplicationDbContext context, IDateTimeProvider clock)
        {
            _context = context;
            _clock = clock;
        }

        public async Task<IReadOnlyList<PaymentDto>> GetByMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
        {
            var payments = await _context.Payments
                .Where(p => p.MemberId == memberId)
                .ToListAsync(cancellationToken);

            // Compute derived figures and order in memory.
            return payments
                .OrderByDescending(p => p.PaidAt)
                .Select(ToDto)
                .ToList();
        }

        public async Task<MonthlyFinancialReportDto> GetMonthlyReportAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            // Fetch only the two columns needed for the aggregation — avoids loading
            // unneeded fields. Filtering by year/month is done in memory because
            var monthPayments = await _context.Payments 
                .Where(p => p.Status == PaymentStatus.Completed
                         && p.PaidAt.Year == year
                         && p.PaidAt.Month == month)
                .Select(p => new { p.Amount, p.VatRate })
                .ToListAsync(cancellationToken);

            var count = monthPayments.Count;    
            var totalGross = monthPayments.Sum(p => p.Amount);
            var totalNet = monthPayments.Sum(p => SplitVat(p.Amount, p.VatRate).net);
            var totalVat = totalGross - totalNet;

            return new MonthlyFinancialReportDto(
                year, month, count,
                totalGross, totalNet, totalVat);
        }

        public async Task<PaymentDto> RecordAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
        {
            // --- The member must exist ---
            var memberExists = await _context.Members
                .AnyAsync(m => m.Id == request.MemberId, cancellationToken);

            if (!memberExists) 
                throw new NotFoundException(nameof(Member), request.MemberId);

            // --- If a membership is referenced, it must exist and belong to this member ---
            if (request.MembershipId is not null)
            {
                var membershipValid = await _context.Memberships
                    .AnyAsync(m => m.Id == request.MembershipId
                                && m.MemberId == request.MemberId,
                                cancellationToken);

                if(!membershipValid)
                    throw new NotFoundException(nameof(Membership), request.MembershipId);
            }

            var payment = new Payment
            {
                MemberId = request.MemberId,
                MembershipId = request.MembershipId,
                Amount = request.Amount,
                VatRate = request.VatRate,
                Method = request.Method,
                Status = PaymentStatus.Completed,   // recorded payments are completed
                PaidAt = _clock.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(cancellationToken);

            return ToDto(payment);
        }

        // Splits a gross amount into (net, vat) using the given rate.
        // net = gross / (1 + rate); vat = gross - net. Rounded to 2 decimals (currency).
        private static (decimal net, decimal vat) SplitVat(decimal gross, decimal rate)
        {
            var net = Math.Round(gross / (1 + rate), 2, MidpointRounding.AwayFromZero);
            var vat = gross - net;
            return (net, vat);
        }

        private static PaymentDto ToDto(Payment p)
        {
            var (net, vat) = SplitVat(p.Amount, p.VatRate);
            return new PaymentDto(
                p.Id, p.MemberId, p.MembershipId,
                p.Amount, net, vat, p.VatRate,
                p.Method.ToString(), p.Status.ToString(), p.PaidAt);
        }
    }
}
