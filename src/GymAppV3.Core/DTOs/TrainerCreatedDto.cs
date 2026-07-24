namespace GymAppV3.Core.DTOs;

/// <summary>
/// Returned only from the create endpoint. Carries the one-time password so the
/// admin can hand it over — this is the single moment it is ever readable.
/// </summary>
public record TrainerCreatedDto(
    TrainerDto Trainer,
    string TemporaryPassword);
