using System.Text;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace GymAppv3.Server.Configuration;

public static class IdentityConfiguration
{
    public static void ConfigureIdentity(this WebApplicationBuilder builder)
    {
        // Configure ASP.NET Core Identity
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            // Password requirements
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;

            // User settings
            options.User.RequireUniqueEmail = true;

            // Lockout settings (optional)
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure Authentication with dual scheme (Cookie + JWT Bearer)
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        })
        .AddJwtBearer(options =>
        {
            var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-change-this-in-production-min-32-chars!";
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GymAppV3";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GymAppV3";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
            };
        });

        // Configure Authorization Policies
        builder.Services.AddAuthorization(options =>
        {
            // Require authenticated user for any endpoint by default
            options.FallbackPolicy = options.DefaultPolicy;

            // Role-based policies
            options.AddPolicy("MemberOnly", policy =>
                policy.RequireRole(RoleConstants.Member));

            options.AddPolicy("TrainerOnly", policy =>
                policy.RequireRole(RoleConstants.Trainer));

            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole(RoleConstants.Admin));

            options.AddPolicy("TrainerAdminOnly", policy =>
                policy.RequireRole(RoleConstants.TrainerAdmin));

            options.AddPolicy("AnyAuthenticated", policy =>
                policy.RequireAuthenticatedUser());
        });
    }
}
