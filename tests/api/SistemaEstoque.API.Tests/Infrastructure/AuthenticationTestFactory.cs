using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SistemaEstoque.Application.Authentication;
using SistemaEstoque.Application.Users;

namespace SistemaEstoque.API.Tests.Infrastructure;

internal sealed class AuthenticationTestFactory(
    Guid identity,
    IUserManagementService? userManagementService = null) : WebApplicationFactory<Program>
{
    internal FakeCurrentAccountAuthorization Authorization { get; } = new(identity);

    internal FakeSupabaseAdminClient IdentityAdministration { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICurrentAccountAuthorization>();
            services.AddSingleton<ICurrentAccountAuthorization>(Authorization);
            services.RemoveAll<IIdentityAdministration>();
            services.AddSingleton<IIdentityAdministration>(IdentityAdministration);
            if (userManagementService is not null)
            {
                services.RemoveAll<IUserManagementService>();
                services.AddSingleton(userManagementService);
            }

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultForbidScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "Test",
                    _ => { });
        });
    }

    private sealed class TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Guid.TryParse(Request.Headers["X-Test-Subject"], out Guid subject))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string statusClaim = Request.Headers["X-Test-Status"].ToString();
            if (string.IsNullOrWhiteSpace(statusClaim))
            {
                statusClaim = "Ativa";
            }
            string roleClaim = Request.Headers["X-Test-Role"].ToString();
            if (string.IsNullOrWhiteSpace(roleClaim))
            {
                roleClaim = "Administrador";
            }
            Guid sessionId = Guid.TryParse(Request.Headers["X-Test-Session"], out Guid parsedSession)
                ? parsedSession
                : Guid.NewGuid();
            ClaimsIdentity identity = new(
            [
                new Claim("sub", subject.ToString()),
                new Claim("session_id", sessionId.ToString()),
                new Claim("account_status", statusClaim),
                new Claim("account_id", subject.ToString()),
                new Claim(ClaimTypes.Role, roleClaim)
            ],
            Scheme.Name);
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}

internal sealed class FakeCurrentAccountAuthorization(Guid identity) : ICurrentAccountAuthorization
{
    private readonly CurrentAccount _account = new(
        Guid.NewGuid(), identity, "user@example.com", "Test User", "Operador", "Ativa");

    public Guid? ActivatedIdentity { get; private set; }

    public string? ActivatedEmail { get; private set; }

    public Guid? RevokedSession { get; private set; }

    public Task<CurrentAccount?> FindByIdentityAsync(Guid requestedIdentity, CancellationToken cancellationToken) =>
        Task.FromResult<CurrentAccount?>(requestedIdentity == identity ? _account : null);

    public Task<bool> IsSessionRevokedAsync(Guid sessionId, CancellationToken cancellationToken) =>
        Task.FromResult(false);

    public Task ActivatePendingAccountAsync(
        Guid requestedIdentity,
        string confirmedEmail,
        string actor,
        CancellationToken cancellationToken)
    {
        ActivatedIdentity = requestedIdentity;
        ActivatedEmail = confirmedEmail;
        return Task.CompletedTask;
    }

    public Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        RevokedSession = sessionId;
        return Task.CompletedTask;
    }
}
