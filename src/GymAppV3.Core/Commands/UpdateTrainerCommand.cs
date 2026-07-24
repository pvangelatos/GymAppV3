namespace GymAppV3.Core.Commands;

public record UpdateTrainerCommand(
    string Firstname,
    string Lastname,
    string Email,
    string? Phone,
    string? Bio,
    IReadOnlyList<Guid> SpecialtyCategoryIds);
