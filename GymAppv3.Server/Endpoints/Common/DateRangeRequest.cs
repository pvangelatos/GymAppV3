namespace GymAppv3.Server.Endpoints.Common;

/// <summary>
/// Shared date-range parameters for range-scoped list endpoints.
/// Both bounds are optional; the service applies defaults from the clock.
/// </summary>
public record DateRangeRequest(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null);
