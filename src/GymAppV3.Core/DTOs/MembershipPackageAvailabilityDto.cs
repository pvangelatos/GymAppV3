namespace GymAppV3.Core.DTOs;

// Live, computed (never persisted) snapshot of how many more memberships of this
// package could be sold without exceeding the category's scheduled capacity.
// AvailableSlots is floor((WeeklySupply - WeeklyDemand) / WeeklyRate), clamped to
// zero — never negative, never re-derived from a stored counter.
public record MembershipPackageAvailabilityDto(
    Guid MembershipPackageId,
    string MembershipPackageName,
    Guid ClassCategoryId,
    string ClassCategoryName,
    double WeeklySupply,
    double WeeklyDemand,
    double WeeklyRate,
    int AvailableSlots);
