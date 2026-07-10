namespace GymAppV3.Core.Models;

public class Trainer : AuditableEntity
{
    // Unique identifier for the trainer record
    public Guid Id { get; set; } = Guid.NewGuid();

    // Reference to the user account associated with this trainer profile
    public required string UserId { get; set; }

    // Trainer's first name
    public required string Firstname { get; set; }

    // Trainer's last name
    public required string Lastname { get; set; }

    // Trainer's email address for contact and communication
    public required string Email { get; set; }

    // Trainer's phone number for direct contact (optional)
    public string? Phone { get; set; }

    // Collection of specialties this trainer is qualified in
    public ICollection<TrainerSpecialty> Specialties { get; set; } = new List<TrainerSpecialty>();

    // Brief biography or professional background of the trainer
    public string? Bio { get; set; }
}
