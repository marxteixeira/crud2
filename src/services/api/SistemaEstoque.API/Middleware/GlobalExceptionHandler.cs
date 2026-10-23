using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace SistemaEstoque.API.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        string route = httpContext.GetEndpoint()?.DisplayName ?? "unmatched";
        string traceId = httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Unhandled {ExceptionType} for {Method} {Route}; TraceId {TraceId}",
            exception.GetType().FullName,
            httpContext.Request.Method,
            route,
            traceId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ProblemDetails problemDetails = new()
        {
            Type = "about:blank",
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError
        };
        problemDetails.Extensions["traceId"] = traceId;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }
}
