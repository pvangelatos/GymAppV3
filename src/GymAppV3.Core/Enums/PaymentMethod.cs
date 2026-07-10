namespace GymAppV3.Core.Enums;

/// <summary>
/// Defines the different payment methods supported by the system
/// </summary>
public enum PaymentMethod
{
    /// <summary>Physical cash payment</summary>
    Cash = 1,

    /// <summary>Credit/debit card payment</summary>
    Card = 2,

    /// <summary>Direct bank transfer payment</summary>
    BankTransfer = 3
}
