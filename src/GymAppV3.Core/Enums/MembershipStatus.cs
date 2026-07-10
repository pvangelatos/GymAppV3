namespace GymAppV3.Core.Enums;

/// <summary>
/// Defines the possible states of a membership subscription
/// </summary>
public enum MembershipStatus
{
    /// <summary>Membership is currently valid and member can attend classes</summary>
    Active = 1,

    /// <summary>Membership has passed its end date and is no longer valid</summary>
    Expired = 2,

    /// <summary>Membership was cancelled by member or system</summary>
    Cancelled = 3,

    /// <summary>Membership is temporarily suspended (e.g., payment pending, member request)</summary>
    Frozen = 4
}
