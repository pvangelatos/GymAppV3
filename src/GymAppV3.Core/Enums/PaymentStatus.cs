

namespace GymAppV3.Core.Enums;

/// <summary>
/// Defines the possible states of a payment transaction
/// </summary>
public enum PaymentStatus
{
    /// <summary>Payment has been initiated but not yet processed</summary>
    Pending = 1,

    /// <summary>Payment has been successfully processed and completed</summary>
    Completed = 2,

    /// <summary>Payment processing failed and was not completed</summary>
    Failed = 3,

    /// <summary>Previously completed payment has been refunded to the member</summary>
    Refunded = 4
}
