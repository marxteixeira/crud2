using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SistemaEstoque.API.Tests;

public sealed class GlobalErrorHandlingTests
{
    [Fact]
    public async Task UnexpectedException_ReturnsProblemDetailsWithoutInternals()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
        using HttpClient client = factory.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Get, "/__test/error");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/problem+json"));

        using HttpResponseMessage response = await client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using JsonDocument problem = JsonDocument.Parse(body);
        Assert.Equal(500, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("An unexpected error occurred.", problem.RootElement.GetProperty("title").GetString());
        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));
        Assert.DoesNotContain("Test-only exception sentinel", body, StringComparison.Ordinal);
        Assert.DoesNotContain("System.InvalidOperationException", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TestOnlyErrorRoute_IsNotMappedOutsideTesting()
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/__test/error");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
