using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.MembershipPackage
{
    // Input for updating a package. Id comes from the route, not the body.
    public record UpdateMembershipPackageRequest(
        string Name,
        decimal Price,
        int DurationInDays,
        int SessionsIncluded);
}
