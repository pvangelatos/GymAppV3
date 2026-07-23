using GymAppV3.Core.DTOs;

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

    /// <summary>
    /// User's address (required for all members)
    /// </summary>
    public required AddressDto Address { get; init; }

    /// <summary>
    /// User's phone number (optional)
    /// </summary>
    public string? Phone { get; init; }

    /// <summary>
    /// User's birth date
    /// </summary>
    public required DateOnly BirthDate { get; init; }

    /// <summary>
    /// Whether the member has medical conditions
    /// </summary>
    public required bool HasMedicalConditions { get; init; }

    /// <summary>
    /// Medical notes (optional, only if HasMedicalConditions is true)
    /// </summary>
    public string? MedicalNotes { get; init; }
}

