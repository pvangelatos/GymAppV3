using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace GymAppv3.Server.Configuration;

public static class RateLimitingConfiguration
{
    public const string AuthLoginPolicy = "auth-login-policy";
    public const string AuthRegisterPolicy = "auth-register-policy";

    public static void ConfigureRateLimiting(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Rejected respones go out as ProblemDetails so the client
            // sees the same shape regardless of what caused the failure.
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString();
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";

                await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "Too Many Requests",
                    Status = StatusCodes.Status429TooManyRequests,
                    Detail = "You have exceeded the allowed number of requests. Please try again later.",
                    Type = "https://tools.ietf.org/html/rfv6585#section-4"
                }, cancellationToken);
            };

            // Login: 5 attempts per minute per IP. Tight because brute - force
            // against known emails it the main attack surface.
            // No queueing: if the user is locked out, they should know immediately.
            options.AddPolicy(AuthLoginPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetIpPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // Register: 3 attempts per 5 minutes per IP. Wider window because
            // registration is one - off action so less frequent and less targeted.
            // No queueing: if the user is locked out, they should know immediately.
            options.AddPolicy(AuthRegisterPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetIpPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0
                    }));

            // Global safety net : 60 requests per minute per user (or IP for anonymous).
            // Applies to every request in addition to any named policy.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: GetIdentityOrIpPartitionKey(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0
                    }));
        });
    }

    // Anonymous requests partition by IP. If behind a reverse proxy in production,
    // this needs UseForwardedHeaders middleware to see the real client IP.
    private static string GetIpPartitionKey(HttpContext context) =>
        $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

    // Authenticated requests partition by userId (so one abusive user can't
    // exhaust another's budget); anonymous fall back to IP.
    private static string GetIdentityOrIpPartitionKey(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrEmpty(userId)
            ? $"user:{userId}"
            : GetIpPartitionKey(context);
    }
}
