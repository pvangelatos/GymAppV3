namespace GymAppV3.Core.Models;

public class ClassSession : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Title { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public int DurationInMinutes { get; set; }
    public int Capacity { get; set; }
    public int AvailableSeats { get; set; }//autocalculated based on capacity and number of members enrolled

    public Guid TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;

    public Guid ClassRoomId { get; set; }
    public ClassRoom ClassRoom { get; set; } = null!;
    public byte[] RowVersion { get; set; } = null!;
}
