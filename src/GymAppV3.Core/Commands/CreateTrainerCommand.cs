namespace GymAppV3.Core.Commands;

/// <summary>
/// Admin-only. Creates the login account and the trainer profile together;
/// the password is generated server-side and returned once.
/// </summary>
public record CreateTrainerCommand(
    string Firstname,
    string Lastname,
    string Email,
    string? Phone,
    string? Bio,
    IReadOnlyList<Guid> SpecialtyCategoryIds);
