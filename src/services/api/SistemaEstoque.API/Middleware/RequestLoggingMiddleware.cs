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
        string route = context.GetEndpoint()?.DisplayName ?? "unmatched";

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
