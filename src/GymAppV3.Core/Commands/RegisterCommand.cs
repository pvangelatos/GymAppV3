using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string Firstname,
    string Lastname,
    string? Phone,
    AddressDto Address,
    DateOnly BirthDate,
    bool HasMedicalConditions,
    string? MedicalNotes);
