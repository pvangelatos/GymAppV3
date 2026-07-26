using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Interfaces;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);
    Task<CurrentUserInfo?> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);
}

// --- Register outcomes ---
public abstract record RegisterResult;
public sealed record RegisterSuccess(string UserId, string Email) : RegisterResult;
public sealed record RegisterEmailInUse : RegisterResult;                   // silent — same 200 to client
public sealed record RegisterFailed(IReadOnlyList<string> Errors) : RegisterResult;

// --- Login outcomes ---
public abstract record LoginResult;
public sealed record LoginSuccess(string Token, string UserId, string Email, IReadOnlyList<string> Roles) : LoginResult;
public sealed record LoginInvalidCredentials : LoginResult;                 // user-not-found OR wrong password
public sealed record LoginLockedOut : LoginResult;

// --- /me outcome ---
public sealed record CurrentUserInfo(string UserId, string Email, IReadOnlyList<string> Roles);
