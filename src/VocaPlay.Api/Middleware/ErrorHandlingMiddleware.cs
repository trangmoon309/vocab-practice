// VocaPlay.Api/Middleware/ErrorHandlingMiddleware.cs
using System.Net;
using System.Text.Json;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message, errors) = ex switch
        {
            NotFoundException => (HttpStatusCode.NotFound, ex.Message, null),
            ForbiddenException => (HttpStatusCode.Forbidden, ex.Message, null),
            ValidationException validationEx => (HttpStatusCode.BadRequest, validationEx.Message, (IReadOnlyList<string>?)validationEx.Errors),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message, null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new Dictionary<string, object?>
        {
            ["message"] = message,
            ["statusCode"] = (int)statusCode
        };

        if (errors is not null)
            payload["errors"] = errors;

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
