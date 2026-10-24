using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SistemaEstoque.Infrastructure.Identity;

namespace SistemaEstoque.API.Tests;

public sealed class SupabaseAdminClientTests
{
    [Fact]
    public async Task Invite_UsesServerAdminEndpointAndReturnsProviderIdentity()
    {
        Guid expectedIdentity = Guid.NewGuid();
        RecordingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { id = expectedIdentity }))
        });
        using HttpClient httpClient = new(handler);
        SupabaseAdminClient client = new(httpClient, CreateConfiguration());

        Guid actualIdentity = await client.InviteAsync(
            "user@example.com",
            "Test User",
            CancellationToken.None);

        RecordedRequest request = Assert.Single(handler.Requests);
        Assert.Equal(expectedIdentity, actualIdentity);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://project.example/auth/v1/invite", request.Uri);
        Assert.Equal("service-role-key-sentinel", request.ApiKey);
        Assert.Contains("\"display_name\":\"Test User\"", request.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("service-role-key-sentinel", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateRoleAndEmailChange_UseExpectedAdminPayloads()
    {
        RecordingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpClient httpClient = new(handler);
        SupabaseAdminClient client = new(httpClient, CreateConfiguration());
        Guid identity = Guid.NewGuid();

        await client.UpdateRoleAsync(identity, "Administrador", CancellationToken.None);
        await client.RequestEmailChangeAsync(identity, "new@example.com", CancellationToken.None);

        Assert.Equal(2, handler.Requests.Count);
        Assert.Contains("\"app_metadata\":{\"role\":\"Administrador\"}", handler.Requests[0].Body, StringComparison.Ordinal);
        Assert.Contains("\"email\":\"new@example.com\"", handler.Requests[1].Body, StringComparison.Ordinal);
        Assert.Contains("\"email_confirm\":false", handler.Requests[1].Body, StringComparison.Ordinal);
        Assert.All(handler.Requests, request =>
        {
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Contains("/auth/v1/admin/users/", request.Uri, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task ConfirmedEmail_UsesCallerTokenAndReturnsOnlyConfirmedAddress()
    {
        RecordingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"email":"user@example.com","email_confirmed_at":"2026-10-23T12:00:00Z"}""")
        });
        using HttpClient httpClient = new(handler);
        SupabaseAdminClient client = new(httpClient, CreateConfiguration());

        string? email = await client.GetConfirmedEmailAsync("caller-access-token", CancellationToken.None);

        RecordedRequest request = Assert.Single(handler.Requests);
        Assert.Equal("user@example.com", email);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.EndsWith("/auth/v1/user", request.Uri, StringComparison.Ordinal);
        Assert.Equal("anon-key-sentinel", request.ApiKey);
        Assert.Equal("Bearer caller-access-token", request.Authorization);
    }

    [Fact]
    public async Task ProviderError_DoesNotEchoResponseBodyOrConfiguredSecret()
    {
        RecordingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("service-role-key-sentinel provider diagnostic")
        });
        using HttpClient httpClient = new(handler);
        SupabaseAdminClient client = new(httpClient, CreateConfiguration());

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.UpdateRoleAsync(Guid.NewGuid(), "Operador", CancellationToken.None));

        Assert.Contains("400", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("service-role-key-sentinel", exception.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("provider diagnostic", exception.ToString(), StringComparison.Ordinal);
    }

    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Supabase:Url"] = "https://project.example",
                ["Supabase:AnonKey"] = "anon-key-sentinel",
                ["Supabase:ServiceRoleKey"] = "service-role-key-sentinel"
            })
            .Build();

    private sealed record RecordedRequest(
        HttpMethod Method,
        string Uri,
        string? ApiKey,
        string? Authorization,
        string Body);

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new RecordedRequest(
                request.Method,
                request.RequestUri?.ToString() ?? string.Empty,
                request.Headers.TryGetValues("apikey", out IEnumerable<string>? keys) ? keys.Single() : null,
                request.Headers.Authorization?.ToString(),
                body));
            return responseFactory(request);
        }
    }
}
