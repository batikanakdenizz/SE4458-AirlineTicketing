using System.Net;
using System.Text.Json;

namespace AirlineTicketing.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, "VALIDATION_ERROR"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "RESOURCE_NOT_FOUND"),
            InvalidOperationException => (HttpStatusCode.Conflict, "BUSINESS_RULE_VIOLATION"),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
        }
        else
        {
            _logger.LogWarning(exception, "Handled request exception.");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = new
            {
                code,
                message = exception.Message
            },
            metadata = new
            {
                requestId = context.TraceIdentifier,
                timestamp = DateTime.UtcNow
            }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
