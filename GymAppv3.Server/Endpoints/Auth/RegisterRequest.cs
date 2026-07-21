namespace GymAppv3.Server.Endpoints.Auth;

/// <summary>
/// Request model for user registration
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// User's email address (will also be the username)
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User's password
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Password confirmation (must match Password)
    /// </summary>
    public required string ConfirmPassword { get; init; }

    /// <summary>
    /// User's first name
    /// </summary>
    public required string Firstname { get; init; }

    /// <summary>
    /// User's last name
    /// </summary>
    public required string Lastname { get; init; }
}
