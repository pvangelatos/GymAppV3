using GymAppV3.Core.Enums;

namespace GymAppV3.Core.Models;

public class Membership : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public Guid MembershipPackageId { get; set; }
    public MembershipPackage MembershipPackage { get; set; } = null!;

    public decimal PricePaid { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int RemainingSessions { get; set; }                       // the counter of seat-slots
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;     // every new membership is active
}
