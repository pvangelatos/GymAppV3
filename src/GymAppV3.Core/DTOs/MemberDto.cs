namespace GymAppV3.Core.DTOs;

/// <summary>
/// Basic member profile without sensitive medical data.
/// Used for general member listings and non-privileged views.
/// </summary>
public record MemberDto(
    Guid Id,
    string? UserId,
    string Firstname,
    string Lastname,
    string Email,
    string? Phone,
    AddressDto Address,
    DateOnly BirthDate,
    bool HasMedicalConditions);
    

