using GymAppV3.Core.Enums;

namespace GymAppV3.Core.Models;

public class Membership : AuditableEntity
{
    // Unique identifier for the membership record
    public Guid Id { get; set; } = Guid.NewGuid();

    // Reference to the member who owns this membership
    public Guid MemberId { get; set; }

    // Navigation property to the Member entity
    public Member Member { get; set; } = null!;

    // Reference to the membership package purchased by the member
    public Guid MembershipPackageId { get; set; }

    // Navigation property to the MembershipPackage entity (contains package details like duration and sessions)
    public MembershipPackage MembershipPackage { get; set; } = null!;

    // The amount paid for this membership subscription
    public decimal PricePaid { get; set; }

    // Date when the membership becomes active
    public DateTimeOffset StartDate { get; set; }

    // Date when the membership expires
    public DateTimeOffset EndDate { get; set; }

    // Counter tracking the number of class session slots still available for this membership
    public int RemainingSessions { get; set; }

    // Current status of the membership (defaults to Active when created)
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;

    // Optimistic concurrency token. Prevents lost updates when two concurrent bookings
    // read the same RemainingSessions value and both decrement it.
    public byte[] RowVersion { get; set; } = null!;
}
