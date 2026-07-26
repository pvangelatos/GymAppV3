namespace GymAppv3.Server.Endpoints.Common;

/// <summary>
/// Shared paging parameters for list endpoints.
/// </summary>
public record PaginationRequest(
    int Page = 1,
    int Size = 50,
    string? Sort = null);
