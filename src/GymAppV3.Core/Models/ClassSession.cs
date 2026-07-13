namespace GymAppV3.Core.Models;

public class ClassSession : AuditableEntity
{
    // Unique identifier for the class session
    public Guid Id { get; set; } = Guid.NewGuid();

    // Title or name of the class (e.g., "Morning Yoga", "CrossFit 101")
    public required string Title { get; set; }

    // Date and time when the class session begins
    public DateTimeOffset StartsAt { get; set; }

    // Date and time when the class session ends
    public DateTimeOffset EndsAt { get; set; }


    // Length of the class session in minutes
    public int DurationInMinutes { get; set; }

    // Maximum number of members allowed in this class session
    public int Capacity { get; set; }

    // Number of available slots remaining (automatically calculated from capacity minus enrolled members)
    public int AvailableSeats { get; set; }

    // Reference to the trainer conducting the class
    public Guid TrainerId { get; set; }

    // Navigation property for the trainer entity
    public Trainer Trainer { get; set; } = null!;

    // Reference to the classroom where the session is held
    public Guid ClassRoomId { get; set; }

    // Navigation property for the classroom entity
    public ClassRoom ClassRoom { get; set; } = null!;

    // Row version for optimistic concurrency control in database updates
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Validates that DurationInMinutes and Capacity are valid values
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (DurationInMinutes <= 0)
            errors.Add("DurationInMinutes must be greater than 0");

        if (Capacity <= 0)
            errors.Add("Capacity must be greater than 0");

        if (AvailableSeats < 0)
            errors.Add("AvailableSeats cannot be negative");

        if (AvailableSeats > Capacity)
            errors.Add("AvailableSeats cannot exceed Capacity");

        return errors.Count == 0;
    }
}
