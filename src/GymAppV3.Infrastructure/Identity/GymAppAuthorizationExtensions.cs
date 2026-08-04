using Microsoft.AspNetCore.Authorization;

namespace GymAppV3.Infrastructure.Identity;

public static class GymAppAuthorizationExtensions
{
    public static void AddGymAppPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy("MemberOnly", policy =>
            policy.RequireRole(RoleConstants.Member));

        options.AddPolicy("TrainerOnly", policy =>
            policy.RequireRole(RoleConstants.Trainer, RoleConstants.TrainerAdmin));

        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole(RoleConstants.Admin, RoleConstants.TrainerAdmin));

        options.AddPolicy("TrainerAdminOnly", policy =>
            policy.RequireRole(RoleConstants.TrainerAdmin));

        options.AddPolicy("StaffOnly", policy =>
            policy.RequireRole(RoleConstants.Admin, RoleConstants.Trainer, RoleConstants.TrainerAdmin));

        options.AddPolicy("AnyAuthenticated", policy =>
            policy.RequireAuthenticatedUser());
    }
}