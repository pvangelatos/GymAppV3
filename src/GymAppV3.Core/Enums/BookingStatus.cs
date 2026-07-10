namespace GymAppV3.Core.Enums;

/// <summary>
/// Defines the possible states of a class booking
/// </summary>
public enum BookingStatus
{
    /// <summary>Booking is confirmed and member is registered for the class</summary>
    Confirmed = 1,

    /// <summary>Booking was cancelled by the member or system</summary>
    Cancelled = 2,

    /// <summary>Class session has been completed</summary>
    Completed = 3,

    /// <summary>Member did not show up for the booked class</summary>
    NoShow = 4
}
