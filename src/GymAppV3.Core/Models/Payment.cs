using GymAppV3.Core.Enums;

namespace GymAppV3.Core.Models;

public class Payment : AuditableEntity
{
    // Unique identifier for the payment record
    public Guid Id { get; set; } = Guid.NewGuid();

    // Reference to the member who made the payment
    public Guid MemberId { get; set; }

    // Navigation property to the Member entity
    public Member Member { get; set; } = null!;

    // Reference to the membership being paid for (optional - payment may not be tied to a specific membership)
    public Guid? MembershipId { get; set; }

    // Navigation property to the Membership entity (nullable)
    public Membership? Membership { get; set; }

    // The payment amount in the system's currency
    public decimal Amount { get; set; }

    // Payment method used (e.g., Cash, Card, BankTransfer)
    public PaymentMethod Method { get; set; }

    // Date and time when the payment was processed
    public DateTimeOffset PaidAt { get; set; }

    // Current status of the payment (defaults to Pending when created)
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

}
