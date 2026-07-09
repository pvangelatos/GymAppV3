namespace GymAppV3.Core.Models;

public class AuditableEntity
{
    public string? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTime.UtcNow;
}
