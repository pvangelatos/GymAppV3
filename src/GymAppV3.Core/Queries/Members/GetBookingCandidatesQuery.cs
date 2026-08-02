namespace GymAppV3.Core.Queries.Members;

public record GetBookingCandidatesQuery(
    Guid ClassCategoryId,
    string? SearchTerm);
