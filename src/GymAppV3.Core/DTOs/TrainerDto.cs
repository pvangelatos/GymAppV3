namespace GymAppV3.Core.DTOs;

public record TrainerDto(
    Guid Id,
    string UserId,
    string Firstname,
    string Lastname,
    string Email,
    string? Phone,
    string? Bio,
    IReadOnlyList<SpecialtyDto> Specialties);
