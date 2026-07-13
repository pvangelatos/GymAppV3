
namespace GymAppV3.Core.Models;

public class MembershipPackage : AuditableEntity
{
    // Unique identifier for the membership package
    public Guid Id { get; set; } = Guid.NewGuid();

    // Name of the membership tier (e.g., "Basic", "Premium", "VIP")
    public required string Name { get; set; }

    // Price of the membership package in the system's currency
    public decimal Price { get; set; }

    // Length of the membership subscription period in days
    public int DurationInDays { get; set; }

    // Number of class sessions included in this membership package
    public int SessionsIncluded { get; set; }

    // The category this package grants access to (e.g. Pilates Reformer). A booking
    // is paid from a membership whose package has the SAME category as the session.
    public Guid ClassCategoryId { get; set; }
    public ClassCategory ClassCategory { get; set; } = null!;
}
