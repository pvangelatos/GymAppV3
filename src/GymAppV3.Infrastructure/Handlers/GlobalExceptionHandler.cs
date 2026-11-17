using GymAppV3.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GymAppV3.Infrastructure.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Business rule violated"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),

            // Optimistic concurrency (RowVersion): another request changed the row first.
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,
                "The resource was changed by another request. Please retry."),

            // Safety net for DB constraints (unique index, FK) that a service pre-check missed,
            // e.g. two concurrent requests creating the same name.
            DbUpdateException => (StatusCodes.Status409Conflict,
                "The request conflicts with existing data."),

            // Client sent something we could not parse or bind.
            BadHttpRequestException or JsonException or FormatException =>
                (StatusCodes.Status400BadRequest, "Malformed request"),
            ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Invalid argument"),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
        else if (statusCode == StatusCodes.Status409Conflict)
            _logger.LogWarning(exception, "Concurrency conflict on {Path}", httpContext.Request.Path);

        // Only our own domain exceptions carry messages that are safe to show the client.
        var detail = exception is NotFoundException or BusinessRuleException or ForbiddenException
            ? exception.Message
            : title;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
