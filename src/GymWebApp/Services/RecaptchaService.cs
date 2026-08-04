using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace GymWebApp.Services;

public class RecaptchaService : IRecaptchaService
{
    private const double MinimumScore = 0.5;

    private readonly HttpClient _httpClient;
    private readonly string _secretKey;
    private readonly ILogger<RecaptchaService> _logger;

    public RecaptchaService(HttpClient httpClient, IConfiguration configuration, ILogger<RecaptchaService> logger)
    {
        _httpClient = httpClient;
        _secretKey = configuration["Recaptcha:SecretKey"]
            ?? throw new InvalidOperationException("Recaptcha:SecretKey is not configured.");
        _logger = logger;
    }

    public async Task<bool> VerifyAsync(string? token, string expectedAction, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var response = await _httpClient.PostAsync(
            "https://www.google.com/recaptcha/api/siteverify",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = _secretKey,
                ["response"] = token
            }),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("reCAPTCHA verification request failed with status {Status}", response.StatusCode);
            return false;
        }

        var result = await response.Content.ReadFromJsonAsync<RecaptchaVerifyResponse>(cancellationToken: cancellationToken);

        if (result is null || !result.Success)
        {
            _logger.LogWarning("reCAPTCHA verification failed: {Errors}", string.Join(", ", result?.ErrorCodes ?? []));
            return false;
        }

        if (!string.Equals(result.Action, expectedAction, StringComparison.Ordinal))
        {
            _logger.LogWarning("reCAPTCHA action mismatch: expected {Expected}, got {Actual}", expectedAction, result.Action);
            return false;
        }

        return result.Score >= MinimumScore;
    }

    private record RecaptchaVerifyResponse(
        bool Success,
        double Score,
        string Action,
        [property: JsonPropertyName("error-codes")] List<string>? ErrorCodes);
}
