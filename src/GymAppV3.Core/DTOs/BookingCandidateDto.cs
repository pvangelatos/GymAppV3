namespace GymAppV3.Core.DTOs;

public record BookingCandidateDto(
    Guid MemberId,
    string FullName,
    bool CanBook,
    string? Reason);
