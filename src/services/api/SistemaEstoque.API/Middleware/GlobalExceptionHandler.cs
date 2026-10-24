using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SistemaEstoque.Application.Users;

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
        int statusCode = exception switch
        {
            AccountConflictException => StatusCodes.Status409Conflict,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled {ExceptionType} for {Method} {Route}; TraceId {TraceId}",
                exception.GetType().FullName,
                httpContext.Request.Method,
                route,
                traceId);
        }
        else
        {
            logger.LogWarning(
                "Request rejected with {StatusCode} for {Method} {Route}; ExceptionType {ExceptionType}; TraceId {TraceId}",
                statusCode,
                httpContext.Request.Method,
                route,
                exception.GetType().FullName,
                traceId);
        }

        httpContext.Response.StatusCode = statusCode;
        ProblemDetails problemDetails = new()
        {
            Type = "about:blank",
            Title = statusCode switch
            {
                StatusCodes.Status404NotFound => "Resource not found.",
                StatusCodes.Status409Conflict => "The request conflicts with the current state.",
                _ => "An unexpected error occurred."
            },
            Detail = statusCode == StatusCodes.Status409Conflict ? exception.Message : null,
            Status = statusCode
        };
        problemDetails.Extensions["traceId"] = traceId;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }
}
