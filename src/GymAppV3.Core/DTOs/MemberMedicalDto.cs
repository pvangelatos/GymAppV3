namespace GymAppV3.Core.DTOs;

public record MemberMedicalDto
(
    Guid Id,
    bool HasMedicalConditions,
    string? MedicalNotes
);
