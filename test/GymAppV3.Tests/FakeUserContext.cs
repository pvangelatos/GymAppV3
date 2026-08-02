using GymAppV3.Core.Abstractions;

namespace GymAppV3.Tests;

// Lets tests set "who is calling" without a real HttpContext.
public class FakeUserContext : IUserContext
{
    public string? UserId { get; private set; }
    public IReadOnlyList<string> Roles { get; private set; } = [];

    public bool IsInRole(string role) => Roles.Contains(role);

    public void As(string userId, params string[] roles)
    {
        UserId = userId;
        Roles = roles;
    }

    public void AsAnonymous()
    {
        UserId = null;
        Roles = [];
    }
}
