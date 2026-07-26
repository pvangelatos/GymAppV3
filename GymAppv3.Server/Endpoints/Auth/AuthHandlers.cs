using System.Security.Claims;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Interfaces;

namespace GymAppv3.Server.Endpoints.Auth;

public static class AuthHandlers
{
    public static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email, request.Password,
            request.Firstname, request.Lastname, request.Phone,
            request.Address, request.BirthDate,
            request.HasMedicalConditions, request.MedicalNotes);

        var result = await authService.RegisterAsync(command, cancellationToken);

        // Both RegisterSuccess and RegisterEmailInUse return the same generic
        // response — the client cannot tell if the email was already taken.
        return result switch
        {
            RegisterSuccess or RegisterEmailInUse => Results.Ok(new AuthResponse
            {
                Success = true,
                Message = "Η αίτηση εγγραφής παραλήφθηκε."
            }),
            RegisterFailed failed => Results.BadRequest(new AuthResponse
            {
                Success = false,
                Message = string.Join(" ", failed.Errors)
            }),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    public static async Task<IResult> LoginAsync(
        LoginRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return result switch
        {
            LoginSuccess ok => Results.Ok(new AuthResponse
            {
                Success = true,
                Message = "Επιτυχής είσοδος.",
                Token = ok.Token,
                UserId = ok.UserId,
                Email = ok.Email,
                Roles = ok.Roles.ToList()
            }),
            LoginLockedOut => Results.BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Ο λογαριασμός είναι προσωρινά κλειδωμένος."
            }),
            // Same response for wrong-password AND user-not-found — no enumeration.
            _ => Results.Unauthorized()
        };
    }

    public static IResult LogoutAsync()
    {
        // JWT is stateless — client-side token disposal is the "logout".
        // For server-side revocation, a refresh-token store is the standard mechanism (out of scope for now).
        return Results.Ok(new AuthResponse
        {
            Success = true,
            Message = "Αποσυνδεθήκατε."
        });
    }

    public static async Task<IResult> GetCurrentUserAsync(
        HttpContext httpContext,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var info = await authService.GetCurrentUserAsync(userId, cancellationToken);
        if (info is null) return Results.NotFound();

        return Results.Ok(new AuthResponse
        {
            Success = true,
            Message = "OK.",
            UserId = info.UserId,
            Email = info.Email,
            Roles = info.Roles.ToList()
        });
    }
}