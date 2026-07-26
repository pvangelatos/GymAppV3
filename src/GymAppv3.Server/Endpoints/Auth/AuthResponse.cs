namespace GymAppv3.Server.Endpoints.Auth;

/// <summary>
/// Response model for authentication operations
/// </summary>
public record AuthResponse
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Message describing the result
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// JWT token (only provided on successful login)
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// User ID (only provided on successful registration/login)
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// User's email (only provided on successful registration/login)
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// User's roles (only provided on successful login)
    /// </summary>
    public IList<string>? Roles { get; init; }
}
