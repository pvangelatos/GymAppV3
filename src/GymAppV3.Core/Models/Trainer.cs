namespace GymAppV3.Core.Models;

public class Trainer : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required int UserId { get; set; }
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public List<string> Specialty { get; set; } = [];
    public string? Bio { get; set; }
}
