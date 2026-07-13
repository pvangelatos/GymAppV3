using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.MembershipPackage
{
    // Read model returned to clients. Deliberately excludes audit and soft-delete
    // fields — those are internal concerns, not part of the public contract.
    public record MembershipPackageDto(
        Guid Id,
        string Name,
        decimal Price,
        int DurationInDays,
        int SessionsIncluded,
        Guid ClassCategoryId,
        string ClassCategoryName);
}
