using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Commands;

/// <summary>
/// Command to update an existing member profile.
/// Member can update their own profile; Admin can update any profile.
/// </summary>
public record UpdateMemberCommand(
    Guid Id,
    string Firstname,
    string Lastname,
    string Email,
    string? Phone,
    AddressDto Address,
    DateOnly BirthDate,
    bool HasMedicalConditions,
    string? MedicalNotes);
