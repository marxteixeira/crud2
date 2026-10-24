using System.Net;
using System.Net.Http.Headers;
using SistemaEstoque.API.Tests.Infrastructure;

namespace SistemaEstoque.API.Tests;

public sealed class AuthenticationEndpointsTests
{
    [Fact]
    public async Task GetCurrent_ReturnsLinkedAccountWithoutAccessToken()
    {
        Guid identity = Guid.NewGuid();
        using AuthenticationTestFactory factory = new(identity);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-access-token");
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());

        using HttpResponseMessage response = await client.GetAsync("/api/auth/me");
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("user@example.com", body, StringComparison.Ordinal);
        Assert.DoesNotContain("test-access-token", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Activate_RequiresProviderConfirmedInviteEmail()
    {
        Guid identity = Guid.NewGuid();
        using AuthenticationTestFactory factory = new(identity);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-access-token");
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Status", "ConvitePendente");
        factory.IdentityAdministration.ConfirmedEmail = "user@example.com";

        using HttpResponseMessage response = await client.PostAsync("/api/auth/activation", content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(identity, factory.Authorization.ActivatedIdentity);
        Assert.Equal("user@example.com", factory.Authorization.ActivatedEmail);
    }

    [Fact]
    public async Task Activate_RejectsUnconfirmedEmailWithoutActivatingAccount()
    {
        Guid identity = Guid.NewGuid();
        using AuthenticationTestFactory factory = new(identity);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-access-token");
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Status", "ConvitePendente");
        factory.IdentityAdministration.ConfirmedEmail = null;

        using HttpResponseMessage response = await client.PostAsync("/api/auth/activation", content: null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Null(factory.Authorization.ActivatedIdentity);
    }

    [Fact]
    public async Task Logout_PersistsSessionRevocationBeforeReturningSuccess()
    {
        Guid identity = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        using AuthenticationTestFactory factory = new(identity);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-access-token");
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Session", sessionId.ToString());

        using HttpResponseMessage response = await client.PostAsync("/api/auth/logout", content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(sessionId, factory.Authorization.RevokedSession);
    }

    [Fact]
    public async Task ConfirmEmailChange_UsesConfirmedProviderEmail()
    {
        Guid identity = Guid.NewGuid();
        UserManagementEndpointsTests.FakeUserManagementService userManagement = new();
        using AuthenticationTestFactory factory = new(identity, userManagement);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-access-token");
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());
        factory.IdentityAdministration.ConfirmedEmail = "new@example.com";

        using HttpResponseMessage response = await client.PostAsync(
            "/api/auth/email-change-confirmation",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(identity, userManagement.ConfirmedIdentity);
        Assert.Equal("new@example.com", userManagement.ConfirmedEmail);
    }

    [Fact]
    public async Task InactiveAccount_CannotReadCurrentProfile()
    {
        Guid identity = Guid.NewGuid();
        using AuthenticationTestFactory factory = new(identity);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Status", "Inativa");

        using HttpResponseMessage response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
