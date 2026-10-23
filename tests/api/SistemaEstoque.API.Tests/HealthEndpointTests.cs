using System.Net;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace SistemaEstoque.API.Tests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_ReturnsOkAndHealthyStatus()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());
        DateTimeOffset timestamp = DateTimeOffset.Parse(
            doc.RootElement.GetProperty("timestamp").GetString()!,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);
        Assert.Equal(TimeSpan.Zero, timestamp.Offset);
    }

    [Fact]
    public async Task Health_AllowsConfiguredCorsOrigin()
    {
        HttpClient client = _factory.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:3000");

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Readiness_ReturnsServiceUnavailableWhenPersistenceIsUnhealthy()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IConfigureOptions<HealthCheckServiceOptions>>();
                    services.AddHealthChecks()
                        .AddCheck<UnavailableReadinessCheck>("postgresql", tags: ["ready"]);
                }));
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/ready");
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.DoesNotContain("not-a-secret", body, StringComparison.Ordinal);
        Assert.DoesNotContain("127.0.0.1", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Readiness_ReturnsOkWhenPostgreSqlIsAvailable()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/ready");
        using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());
    }

    private sealed class UnavailableReadinessCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(HealthCheckResult.Unhealthy("Required persistence is unavailable."));
    }
}
