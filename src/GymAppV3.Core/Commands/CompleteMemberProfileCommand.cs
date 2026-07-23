using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Commands;

public record CompleteMemberProfileCommand(
    string Firstname,
    string Lastname,
    string Email,
    string? Phone,
    AddressDto Address,
    DateOnly BirthDate,
    bool HasMedicalConditions,
    string? MedicalNotes
);
