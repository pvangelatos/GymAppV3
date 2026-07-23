namespace GymAppV3.Core.DTOs;

/// <summary>
/// Detailed member profile including sensitive medical data.
/// Used when the requesting user has authorization to view medical information.
/// Authorization: Member (own profile), Admin (all), Trainer (members with booked sessions).
/// </summary>
public record MemberDetailDto(
    Guid Id,
    string? UserId,
    string Firstname,
    string Lastname,
    string Email,
    string? Phone,
    AddressDto Address,
    DateOnly BirthDate,
    bool HasMedicalConditions,
    string? MedicalNotes);
