namespace SistemaEstoque.API.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? displayName = context.GetEndpoint()?.DisplayName;
        string route = string.IsNullOrEmpty(displayName)
            || displayName.StartsWith("405 HTTP Method Not Supported", StringComparison.Ordinal)
                ? $"unmatched {context.Request.Path}"
                : displayName;

        try
        {
            await _next(context);
        }
        catch
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            _logger.LogInformation(
                "HTTP {Method} {Route} responded with {StatusCode}; TraceId {TraceId}",
                context.Request.Method,
                route,
                context.Response.StatusCode,
                context.TraceIdentifier);
        }
    }
}
