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
    /// Service for managing payment transactions reporting.
    /// Handles payment validation, VAT calculation snapshots, and transaction persistence.
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
}
