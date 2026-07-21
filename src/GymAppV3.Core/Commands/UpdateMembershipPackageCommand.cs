
namespace GymAppV3.Core.Commands;

// Input for updating a package. Id comes from the route, not the body.
public record UpdateMembershipPackageCommand(
    string Name,
    decimal Price,
    int DurationInDays,
    int SessionsIncluded,
    Guid ClassCategoryId);
