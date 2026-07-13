using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.MembershipPackage
{
    // Input for creating a package. No Id — the server generates it.
    public record CreateMembershipPackageRequest(
        string Name,
        decimal Price,
        int DurationInDays,
        int SessionsIncluded,
        Guid ClassCategoryId);
}
