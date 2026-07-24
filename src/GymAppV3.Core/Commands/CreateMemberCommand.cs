using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Commands;

/// <summary>
/// Command for Admin/Trainer to create an offline member profile — a ledger entry
/// with no login account. UserId is always null; self-service profiles are created
/// through CompleteMemberProfileCommand instead.
/// </summary>
public record CreateMemberCommand(
    string Firstname,
    string Lastname,
    string Email,
    string? Phone,
    AddressDto Address,
    DateOnly BirthDate,
    bool HasMedicalConditions,
    string? MedicalNotes);
