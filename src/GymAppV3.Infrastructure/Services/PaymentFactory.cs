using GymAppV3.Core.Common;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Models;

namespace GymAppV3.Infrastructure.Services;

// Single place that builds a Payment with its VAT snapshot.
internal static class PaymentFactory
{
    public static Payment Create(
        Guid memberId, Guid? membershipId, decimal amount, PaymentMethod method,
        decimal vatRate, PaymentStatus status, DateTimeOffset now)
    {
        var (net, _) = VatCalculator.Split(amount, vatRate);
        return new Payment
        {
            MemberId = memberId,
            MembershipId = membershipId,
            Amount = amount,
            VatRate = vatRate,
            NetAmount = net,
            Method = method,
            Status = status,
            PaidAt = now
        };
    }
}
