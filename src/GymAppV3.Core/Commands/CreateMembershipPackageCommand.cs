using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Commands
{
    // Input for creating a package. No Id — the server generates it.
    public record CreateMembershipPackageCommand(
        string Name,
        decimal Price,
        int DurationInDays,
        int SessionsIncluded,
        Guid ClassCategoryId);
}
