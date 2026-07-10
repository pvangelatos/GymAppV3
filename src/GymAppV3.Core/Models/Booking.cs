using GymAppV3.Core.Enums;

namespace GymAppV3.Core.Models;

public class Booking : AuditableEntity
{
    // Unique identifier for the booking record
    public Guid Id { get; set; } = Guid.NewGuid();

    // Reference to the member who made the booking
    public Guid MemberId { get; set; } 

    // Navigation property to the Member entity
    public required Member Member { get; set; }

    // Reference to the class session being booked
    public Guid ClassSessionId { get; set; } 

    // Navigation property to the ClassSession entity
    public required ClassSession ClassSession { get; set; }

    // Current status of the booking (e.g., Confirmed, Cancelled, Pending)
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    // Timestamp recording when the booking was created
    public DateTimeOffset BookedAt { get; set; }

    // Timestamp recording when the booking was cancelled (null if booking is still active)
    public DateTimeOffset? CancelledAt { get; set; }
}
