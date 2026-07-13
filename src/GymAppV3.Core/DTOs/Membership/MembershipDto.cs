using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.Membership
{
    // Read model for a membership. Includes the package name so clients can display
    // it without a second lookup, and the frozen PricePaid rather than the package's
    // current price.
    public record MembershipDto(
        Guid Id,
        Guid MemberId,
        Guid MembershipPackageId,
        string PackageName,
        decimal PricePaid,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate,
        int RemainingSessions,
        string Status);
}
