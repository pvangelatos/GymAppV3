namespace GymWebApp.Services;

public interface IRecaptchaService
{
    Task<bool> VerifyAsync(string? token, string expectedAction, CancellationToken cancellationToken = default);
}
