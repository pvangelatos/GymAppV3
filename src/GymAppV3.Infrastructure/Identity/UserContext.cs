using System.Security.Claims;
using GymAppV3.Core.Abstractions;
using Microsoft.AspNetCore.Http;

namespace GymAppV3.Infrastructure.Identity;

/// <summary>
/// Implementation of IUserContext that retrieves user information from HTTP context claims
/// </summary>
public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? CurrentUser
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true ? user : null;
        }
    }

    /// <summary>
    /// Gets the ID of the currently authenticated user from claims
    /// Returns null if no user is authenticated
    /// </summary>
    public string? UserId =>
        // NameIdentifier is the standard claim type for user ID in ASP.NET Core Identity
        CurrentUser?.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Role claims are written into the token at login, so this needs no database round trip.
    /// </summary>
    public IReadOnlyList<string> Roles =>
        CurrentUser?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public bool IsInRole(string role) => CurrentUser?.IsInRole(role) == true;
}
