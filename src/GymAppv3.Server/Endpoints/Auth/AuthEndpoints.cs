using GymAppv3.Server.Configuration;
using GymAppv3.Server.Endpoints.Common;

namespace GymAppv3.Server.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/register", AuthHandlers.RegisterAsync)
            .WithName("Register")
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitingConfiguration.AuthRegisterPolicy)
            .AddEndpointFilter<ValidationFilter<RegisterRequest>>()
            .Accepts<RegisterRequest>("application/json")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status429TooManyRequests);

        group.MapPost("/login", AuthHandlers.LoginAsync)
            .WithName("Login")
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitingConfiguration.AuthLoginPolicy)
            .AddEndpointFilter<ValidationFilter<LoginRequest>>()
            .Accepts<LoginRequest>("application/json")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status429TooManyRequests);

        group.MapPost("/logout", AuthHandlers.LogoutAsync)
            .WithName("Logout")
            .RequireAuthorization()
            .Produces<AuthResponse>(StatusCodes.Status200OK);

        group.MapGet("/me", AuthHandlers.GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .RequireAuthorization()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}