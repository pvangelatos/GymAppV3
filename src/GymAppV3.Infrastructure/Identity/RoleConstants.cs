namespace GymAppV3.Infrastructure.Identity;

/// <summary>
/// Constants for role names used in the application
/// </summary>
public static class RoleConstants
{
    /// <summary>
    /// Member role - default role for self-registered users
    /// </summary>
    public const string Member = "Member";

    /// <summary>
    /// Trainer role - for gym trainers
    /// </summary>
    public const string Trainer = "Trainer";

    /// <summary>
    /// Admin role - for system administrators
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// TrainerAdmin role - for trainers with administrative privileges
    /// </summary>
    public const string TrainerAdmin = "TrainerAdmin";
}
