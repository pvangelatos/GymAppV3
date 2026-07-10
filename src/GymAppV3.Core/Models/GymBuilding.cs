namespace GymAppV3.Core.Models;

public class GymBuilding : AuditableEntity
{
    // Unique identifier for the gym building
    public Guid Id { get; set; } = Guid.NewGuid();

    // Name of the gym facility (e.g., "Downtown Gym", "Central Fitness")
    public required string Name { get; set; }

    // Optional description or details about the gym
    public string? Description { get; set; }

    // Physical location and address details of the gym
    public required Address Address { get; set; }

    // Contact phone number for the gym facility
    public string? Phone { get; set; }

    // Contact email address for gym inquiries
    public string? Email { get; set; }
}
