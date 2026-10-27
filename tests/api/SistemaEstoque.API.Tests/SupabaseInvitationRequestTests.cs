using System.Text;
using Microsoft.Extensions.Configuration;
using SistemaEstoque.Infrastructure.Identity;

namespace SistemaEstoque.API.Tests;

public sealed class SupabaseInvitationRequestTests
{
    [Fact]
    public async Task InviteAsync_SendsConfirmRouteAsRedirectTo()
    {
        CapturingHandler handler = new();
        SupabaseAdminClient client = CreateClient(handler, "http://localhost:3000/");

        await client.InviteAsync("user@example.com", "User", CancellationToken.None);

        Assert.NotNull(handler.Request);
        Assert.Equal("/auth/v1/invite", handler.Request.RequestUri!.AbsolutePath);
        Assert.Contains(
            "redirect_to=" + Uri.EscapeDataString("http://localhost:3000/auth/confirm"),
            handler.Request.RequestUri.Query,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task InviteAsync_WithoutRedirectUrlOmitsRedirectTo()
    {
        CapturingHandler handler = new();
        SupabaseAdminClient client = CreateClient(handler, string.Empty);

        await client.InviteAsync("user@example.com", "User", CancellationToken.None);

        Assert.Equal(string.Empty, handler.Request!.RequestUri!.Query);
    }

    private static SupabaseAdminClient CreateClient(HttpMessageHandler handler, string redirectUrl)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Supabase:Url"] = "https://example.supabase.co",
                ["Supabase:ServiceRoleKey"] = "service-key",
                ["Supabase:RedirectUrl"] = redirectUrl
            })
            .Build();
        return new SupabaseAdminClient(new HttpClient(handler), configuration);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $"{{\"id\":\"{Guid.NewGuid()}\"}}",
                    Encoding.UTF8,
                    "application/json")
            });
        }
    }
}
