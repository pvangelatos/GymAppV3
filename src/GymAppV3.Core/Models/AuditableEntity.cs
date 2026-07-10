namespace GymAppV3.Core.Models;

public abstract class AuditableEntity
{
    // Tracks the user ID who originally created the record
    public string? CreatedBy { get; set; }

    // Tracks when the record was first created - timestamp is set at the time of entity instantiation
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;

    // Tracks the user ID who last modified the record
    public string? UpdatedBy { get; set; }

    // Tracks when the record was last updated - timestamp is set at the time of entity instantiation
    public DateTimeOffset UpdatedAt { get; set; } = DateTime.UtcNow;

    // Soft delete marker. Rows are never physically removed — they are hidden by
    // a global query filter. Preserves referential integrity and audit history.
    public bool IsDeleted { get; set; }

    // When the row was soft-deleted (null while active).
    public DateTimeOffset? DeletedAt { get; set; }

    // Who soft-deleted the row (null while active).
    public string? DeletedBy { get; set; }
}
