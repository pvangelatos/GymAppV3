
namespace GymAppV3.Core.DTOs;

// Read model for a payment. Exposes the gross amount plus the derived net and VAT
// figures, so a client (or receipt) doesn't have to recompute them.
public record PaymentDto(
    Guid Id,
    Guid MemberId,
    Guid? MembershipId,
    decimal Amount,            // gross
    decimal NetAmount,         // gross without VAT
    decimal VatAmount,         // the VAT portion
    decimal VatRate,
    string Method,
    string Status,
    DateTimeOffset PaidAt);
