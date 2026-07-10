namespace GymAppV3.Core.Models;

public class Member : AuditableEntity
{
    // Unique identifier for the member record
    public Guid Id { get; set; } = Guid.NewGuid();

    // Reference to the user account associated with this member profile
    public required string UserId { get; set; }

    // Member's first name
    public required string Firstname { get; set; }

    // Member's last name
    public required string Lastname { get; set; }

    // Member's email address for contact and communication
    public required string Email { get; set; }

    // Member's phone number for contact (optional)
    public string? Phone { get; set; }

    // Member's address
    public required Address? Address { get; set; }

    // Member's BirthDate
    public required DateOnly BirthDate { get; set; }

    // Explicit declaration — member must consciously answer, cannot skip.
    public required bool HasMedicalConditions { get; set; }

    // Health details. Required only when HasMedicalConditions is true (enforced in validation layer).
    // Sensitive data (GDPR Art. 9) — exposed only to the member and to trainers of their booked sessions.
    public string? MedicalNotes { get; set; }

}
