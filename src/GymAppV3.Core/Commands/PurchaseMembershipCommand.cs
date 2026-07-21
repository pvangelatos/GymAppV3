

namespace GymAppV3.Core.Commands;

// Input for purchasing a membership. The client supplies only *who* and *which
// package* — everything else (dates, price, sessions, status) is derived by the
// service from the package. The client cannot dictate the price or the balance.
public record PurchaseMembershipCommand(
    Guid MemberId,
    Guid MembershipPackageId);

