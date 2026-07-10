namespace GymAppV3.Infrastructure.Abstractions;

/// <summary>
/// Abstraction for getting the current UTC time
/// Enables testability and consistent timezone handling across the application
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current date and time in UTC timezone
    /// Used instead of DateTime.UtcNow to allow for mocking in tests
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
