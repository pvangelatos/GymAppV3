using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Models;

namespace GymAppV3.Infrastructure.Identity;

/// <summary>
/// Shared authorization checks built on IUserContext. Extracted from the near-identical
/// private helpers duplicated in MemberService/TrainerService (RequireAuthenticatedUserId /
/// EnsureIsAdmin / EnsureIsAdminOrTrainer).
/// </summary>
public static class UserContextAuthorizationExtensions
{
    public static string RequireUserId(this IUserContext userContext)
        => userContext.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");

    public static bool IsStaff(this IUserContext userContext) =>
        userContext.IsInRole(RoleConstants.Admin) ||
        userContext.IsInRole(RoleConstants.Trainer) ||
        userContext.IsInRole(RoleConstants.TrainerAdmin);

    public static bool IsAdmin(this IUserContext userContext) =>
        userContext.IsInRole(RoleConstants.Admin);

    public static void EnsureIsAdmin(this IUserContext userContext)
    {
        userContext.RequireUserId();
        if (!userContext.IsAdmin())
            throw new ForbiddenException("Only administrators can perform this operation.");
    }

    public static void EnsureIsStaff(this IUserContext userContext)
    {
        userContext.RequireUserId();
        if (!userContext.IsStaff())
            throw new ForbiddenException("Only administrators or trainers can perform this operation.");
    }

    /// <summary>
    /// Staff can act on behalf of any member. A non-staff caller may only act on
    /// their own Member record.
    /// </summary>
    public static void EnsureCanActOnBehalfOfMember(this IUserContext userContext, Member member)
    {
        var userId = userContext.RequireUserId();

        if (userContext.IsStaff())
            return;

        if (member.UserId != userId)
            throw new ForbiddenException("You are not allowed to act on behalf of this member.");
    }
}
