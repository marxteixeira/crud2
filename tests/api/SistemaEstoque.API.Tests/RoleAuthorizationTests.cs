using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SistemaEstoque.API.Configuration;
using SistemaEstoque.Application.Authentication;
using SistemaEstoque.Application.Users;

namespace SistemaEstoque.API.Tests;

public sealed class RoleAuthorizationTests
{
    private const string Issuer = "https://project.example/auth/v1";
    private const string Audience = "authenticated";
    private const string SigningSecret = "test-signing-key-that-is-long-enough-for-hs256";

    [Fact]
    public async Task Me_RejectsMissingToken()
    {
        AccountState state = new(Guid.NewGuid(), Guid.NewGuid(), "Operador", "Ativa");
        using JwtApiFactory factory = new(state);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("malformed")]
    [InlineData("signature")]
    [InlineData("issuer")]
    [InlineData("audience")]
    [InlineData("expired")]
    public async Task Me_RejectsInvalidTokens(string invalidTokenType)
    {
        AccountState state = new(Guid.NewGuid(), Guid.NewGuid(), "Operador", "Ativa");
        using JwtApiFactory factory = new(state);
        using HttpClient client = factory.CreateClient();
        string token = invalidTokenType == "malformed"
            ? "not-a-jwt"
            : CreateToken(
                state.Identity,
                state.SessionId,
                invalidTokenType == "issuer" ? "https://wrong.example" : Issuer,
                invalidTokenType == "audience" ? "wrong-audience" : Audience,
                invalidTokenType == "expired" ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.AddMinutes(5),
                invalidTokenType == "signature" ? "different-signing-secret-with-sufficient-length" : SigningSecret);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SameToken_UsesCurrentDatabaseRoleAndRejectsRevokedSession()
    {
        AccountState state = new(Guid.NewGuid(), Guid.NewGuid(), "Operador", "Ativa");
        using JwtApiFactory factory = new(state);
        using HttpClient client = factory.CreateClient();
        string token = CreateToken(
            state.Identity,
            state.SessionId,
            Issuer,
            Audience,
            DateTime.UtcNow.AddMinutes(5),
            SigningSecret,
            includeStaleAdministratorRole: true);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage operatorResponse = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Forbidden, operatorResponse.StatusCode);

        state.SetAccount("Administrador", "Ativa");
        using HttpResponseMessage administratorResponse = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, administratorResponse.StatusCode);

        using HttpResponseMessage logoutResponse = await client.PostAsync("/api/auth/logout", content: null);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        using HttpResponseMessage revokedResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, revokedResponse.StatusCode);
    }

    [Fact]
    public async Task SameToken_IsDeniedAfterAccountBecomesInactive()
    {
        AccountState state = new(Guid.NewGuid(), Guid.NewGuid(), "Operador", "Ativa");
        using JwtApiFactory factory = new(state);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateToken(
                state.Identity,
                state.SessionId,
                Issuer,
                Audience,
                DateTime.UtcNow.AddMinutes(5),
                SigningSecret));

        using HttpResponseMessage activeResponse = await client.GetAsync("/api/auth/me");
        state.SetAccount("Operador", "Inativa");
        using HttpResponseMessage inactiveResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, activeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, inactiveResponse.StatusCode);
    }

    [Fact]
    public async Task PendingAccount_CannotAccessAuthenticatedUserEndpoints()
    {
        AccountState state = new(Guid.NewGuid(), Guid.NewGuid(), "Operador", "ConvitePendente");
        using JwtApiFactory factory = new(state);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateToken(
                state.Identity,
                state.SessionId,
                Issuer,
                Audience,
                DateTime.UtcNow.AddMinutes(5),
                SigningSecret));

        using HttpResponseMessage response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static string CreateToken(
        Guid identity,
        Guid sessionId,
        string issuer,
        string audience,
        DateTime expires,
        string signingSecret,
        bool includeStaleAdministratorRole = false)
    {
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, identity.ToString()),
            new Claim("session_id", sessionId.ToString())
        ];
        if (includeStaleAdministratorRole)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Administrador"));
        }

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(signingSecret));
        JwtSecurityToken token = new(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class JwtApiFactory(AccountState state) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ICurrentAccountAuthorization>();
                services.AddSingleton<ICurrentAccountAuthorization>(state);
                services.RemoveAll<IUserManagementService>();
                services.AddSingleton<IUserManagementService>(
                    new UserManagementEndpointsTests.FakeUserManagementService());
                IConfiguration configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Supabase:Url"] = "https://project.example",
                        ["Supabase:Audience"] = Audience,
                        ["Supabase:AnonKey"] = "test-anon-key",
                        ["Supabase:ServiceRoleKey"] = "test-service-role-key"
                    })
                    .Build();
                services.AddSupabaseAuthentication(configuration, new JwtTestEnvironment());
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(
                        new OpenIdConnectConfiguration { Issuer = Issuer });
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        RequireSignedTokens = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningSecret)),
                        ValidateIssuer = true,
                        ValidIssuer = Issuer,
                        ValidateAudience = true,
                        ValidAudience = Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = JwtRegisteredClaimNames.Sub,
                        RoleClaimType = ClaimTypes.Role
                    };
                });
            });
        }

        private sealed class JwtTestEnvironment : IHostEnvironment
        {
            public string EnvironmentName { get; set; } = Environments.Development;

            public string ApplicationName { get; set; } = nameof(RoleAuthorizationTests);

            public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }
    }

    private sealed class AccountState(
        Guid identity,
        Guid sessionId,
        string role,
        string status) : ICurrentAccountAuthorization
    {
        private readonly HashSet<Guid> _revokedSessions = [];
        private CurrentAccount _account = new(Guid.NewGuid(), identity, "user@example.com", "Test User", role, status);

        public Guid Identity { get; } = identity;

        public Guid SessionId { get; } = sessionId;

        public void SetAccount(string currentRole, string currentStatus) =>
            _account = _account with { Role = currentRole, Status = currentStatus };

        public Task<CurrentAccount?> FindByIdentityAsync(Guid requestedIdentity, CancellationToken cancellationToken) =>
            Task.FromResult<CurrentAccount?>(requestedIdentity == Identity ? _account : null);

        public Task<bool> IsSessionRevokedAsync(Guid requestedSessionId, CancellationToken cancellationToken) =>
            Task.FromResult(_revokedSessions.Contains(requestedSessionId));

        public Task ActivatePendingAccountAsync(
            Guid requestedIdentity,
            string confirmedEmail,
            string actor,
            CancellationToken cancellationToken)
        {
            if (requestedIdentity == Identity)
            {
                SetAccount(_account.Role, "Ativa");
            }

            return Task.CompletedTask;
        }

        public Task RevokeSessionAsync(Guid requestedSessionId, CancellationToken cancellationToken)
        {
            _revokedSessions.Add(requestedSessionId);
            return Task.CompletedTask;
        }
    }
}
