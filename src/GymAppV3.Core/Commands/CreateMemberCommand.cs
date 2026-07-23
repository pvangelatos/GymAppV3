using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Commands;

/// <summary>
/// Command to create a new member profile.
/// Can be used by Admin/Trainer to create members without user accounts (UserId = null).
/// </summary>
public record CreateMemberCommand(
    string? UserId,
    string Firstname,
    string Lastname,
    string Email,
    string? Phone,
    AddressDto Address,
    DateOnly BirthDate,
    bool HasMedicalConditions,
    string? MedicalNotes);
