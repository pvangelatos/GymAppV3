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

    /// <summary>
    /// Gets the ID of the currently authenticated user from claims
    /// Returns null if no user is authenticated
    /// </summary>
    public string? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // NameIdentifier is the standard claim for user ID in ASP.NET Core Identity
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
