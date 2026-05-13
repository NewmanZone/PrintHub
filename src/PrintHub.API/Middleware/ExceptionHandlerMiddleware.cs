namespace PrintHub.API.Middleware;

/// <summary>
/// Global exception handling middleware for consistent error responses
/// </summary>
public class ExceptionHandlerMiddleware
{
    private static readonly Action<ILogger, Exception?> LogUnhandledException =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(LogUnhandledException)),
            "Unhandled exception occurred");

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
            LogUnhandledException(_logger, ex);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                context.Response.StatusCode = 401;
                response.Error = "Unauthorized";
                response.Message = "You are not authorized to access this resource";
                break;

            case ArgumentException argEx:
                context.Response.StatusCode = 400;
                response.Error = "Bad Request";
                response.Message = argEx.Message;
                break;

            case KeyNotFoundException:
                context.Response.StatusCode = 404;
                response.Error = "Not Found";
                response.Message = "The requested resource was not found";
                break;

            default:
                context.Response.StatusCode = 500;
                response.Error = "Internal Server Error";
                response.Message = "An unexpected error occurred. Please try again later.";
                break;
        }

        await context.Response.WriteAsJsonAsync(response);
    }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
