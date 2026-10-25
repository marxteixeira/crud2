using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace SistemaEstoque.API.Tests;

public sealed class StructuredLoggingTests
{
    [Fact]
    public async Task RequestAndFailureLogs_ContainSafeStructuredContext()
    {
        using CapturingLoggerProvider provider = new();
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureLogging(logging => logging.AddProvider(provider));
            });
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage healthResponse = await client.GetAsync("/health");
        const string requestSecret = "request-secret-sentinel";
        using HttpRequestMessage errorRequest = new(HttpMethod.Get, $"/__test/error?token={requestSecret}")
        {
            Content = new StringContent("body-secret-sentinel")
        };
        using HttpResponseMessage errorResponse = await client.SendAsync(errorRequest);

        Assert.Equal(System.Net.HttpStatusCode.OK, healthResponse.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, errorResponse.StatusCode);

        CapturedLog requestLog = Assert.Single(provider.Logs.Where(log =>
            log.Category.EndsWith("RequestLoggingMiddleware", StringComparison.Ordinal)
            && log.Values.ContainsKey("Method")
            && Equals(log.Values["Method"], "GET")
            && Equals(log.Values["StatusCode"], 200)));
        Assert.Contains("Route", requestLog.Values.Keys);
        Assert.Contains("TraceId", requestLog.Values.Keys);

        CapturedLog failedRequestLog = Assert.Single(provider.Logs.Where(log =>
            log.Category.EndsWith("RequestLoggingMiddleware", StringComparison.Ordinal)
            && log.Values.ContainsKey("StatusCode")
            && Equals(log.Values["StatusCode"], 500)));
        Assert.Contains("TraceId", failedRequestLog.Values.Keys);

        CapturedLog failureLog = Assert.Single(provider.Logs.Where(log =>
            log.Category.EndsWith("GlobalExceptionHandler", StringComparison.Ordinal)));
        Assert.Contains("ExceptionType", failureLog.Values.Keys);
        Assert.Contains("TraceId", failureLog.Values.Keys);
        Assert.DoesNotContain("Test-only exception sentinel", failureLog.Message, StringComparison.Ordinal);
        Assert.Contains("Test-only exception sentinel", failureLog.Exception?.ToString(), StringComparison.Ordinal);
        foreach (CapturedLog log in provider.Logs.Where(log =>
                     log.Category.EndsWith("RequestLoggingMiddleware", StringComparison.Ordinal)
                     || log.Category.EndsWith("GlobalExceptionHandler", StringComparison.Ordinal)))
        {
            Assert.DoesNotContain(requestSecret, log.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("body-secret-sentinel", log.Message, StringComparison.Ordinal);
            Assert.DoesNotContain(requestSecret, string.Join(" ", log.Values.Values), StringComparison.Ordinal);
            Assert.DoesNotContain("body-secret-sentinel", string.Join(" ", log.Values.Values), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task CorsPreflight_LogsRequestPathInsteadOfMethodNotSupportedRoute()
    {
        using CapturingLoggerProvider provider = new();
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureLogging(logging => logging.AddProvider(provider));
            });
        using HttpClient client = factory.CreateClient();
        using HttpRequestMessage preflight = new(HttpMethod.Options, "/api/locations/racks");
        preflight.Headers.Add("Origin", "http://localhost:3000");
        preflight.Headers.Add("Access-Control-Request-Method", "POST");

        using HttpResponseMessage response = await client.SendAsync(preflight);

        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        CapturedLog log = Assert.Single(provider.Logs.Where(entry =>
            entry.Category.EndsWith("RequestLoggingMiddleware", StringComparison.Ordinal)
            && Equals(entry.Values["Method"], "OPTIONS")));
        string route = Assert.IsType<string>(log.Values["Route"]);
        Assert.Contains("/api/locations/racks", route, StringComparison.Ordinal);
        Assert.DoesNotContain("405", route, StringComparison.Ordinal);
    }

    private sealed record CapturedLog(
        string Category,
        string Message,
        IReadOnlyDictionary<string, object?> Values,
        Exception? Exception);

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<CapturedLog> _logs = new();

        public IReadOnlyCollection<CapturedLog> Logs => _logs.ToArray();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, this);

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(string category, CapturingLoggerProvider provider) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                Dictionary<string, object?> values = state is IEnumerable<KeyValuePair<string, object?>> properties
                    ? properties.ToDictionary(property => property.Key, property => property.Value)
                    : [];
                provider._logs.Enqueue(new CapturedLog(category, formatter(state, exception), values, exception));
            }
        }
    }
}
