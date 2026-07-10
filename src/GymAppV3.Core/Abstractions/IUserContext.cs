namespace GymAppV3.Infrastructure.Abstractions;

/// <summary>
/// Abstraction for accessing the current authenticated user information
/// Enables separation of concerns between business logic and authentication
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the ID of the currently authenticated user
    /// Returns null if no user is authenticated (anonymous request)
    /// </summary>
    string? UserId { get; }
}
