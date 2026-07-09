using GymAppV3.Core.Enums;

namespace GymAppV3.Core.Models;

public class Booking : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MemberId { get; set; } = Guid.NewGuid();
    public Member Member { get; set; } = null!;

    public Guid ClassSessionId { get; set; } = Guid.NewGuid();
    public ClassSession ClassSession { get; set; } = null!;

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public DateTimeOffset BookedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
}
