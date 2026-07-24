namespace GymAppV3.Core.Abstractions;

/// <summary>
/// Abstraction for accessing the current authenticated user information
/// Everything here comes from the token claims — no database access.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the ID of the currently authenticated user
    /// Returns null if no user is authenticated (anonymous request)
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Roles carried by the current token. Empty when anonymous.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// True when the current token carries the given role.
    /// </summary>
    bool IsInRole(string role);
}
