using Microsoft.EntityFrameworkCore;
using SistemaEstoque.Application.Authentication;
using SistemaEstoque.Application.Users;
using SistemaEstoque.Domain.Entities;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.API.Tests;

public sealed class CurrentAccountAuthorizationTests
{
    [Fact]
    public async Task RevokeSession_PersistsRevocationForSubsequentRequests()
    {
        string databaseName = Guid.NewGuid().ToString();
        Guid sessionId = Guid.NewGuid();
        await using (SistemaEstoqueDbContext context = CreateContext(databaseName))
        {
            CurrentAccountAuthorization authorization = new(context);
            await authorization.RevokeSessionAsync(sessionId, CancellationToken.None);
        }

        await using SistemaEstoqueDbContext nextRequestContext = CreateContext(databaseName);
        CurrentAccountAuthorization nextRequestAuthorization = new(nextRequestContext);

        Assert.True(await nextRequestAuthorization.IsSessionRevokedAsync(sessionId, CancellationToken.None));
    }

    [Fact]
    public async Task RevokeSession_IsIdempotentForAlreadyRevokedSession()
    {
        await using SistemaEstoqueDbContext context = CreateContext(Guid.NewGuid().ToString());
        CurrentAccountAuthorization authorization = new(context);
        Guid sessionId = Guid.NewGuid();

        await authorization.RevokeSessionAsync(sessionId, CancellationToken.None);
        await authorization.RevokeSessionAsync(sessionId, CancellationToken.None);

        Assert.Equal(1, await context.SessoesRevogadas.CountAsync());
    }

    [Fact]
    public async Task ActivatePendingAccount_RequiresConfirmedEmailToMatchInvite()
    {
        await using SistemaEstoqueDbContext context = CreateContext(Guid.NewGuid().ToString());
        Guid identity = Guid.NewGuid();
        context.ContasDeUsuario.Add(ContaDeUsuario.CriarConvite(
            identity,
            "invited@example.com",
            "Invited User",
            PerfilUsuario.Operador,
            "admin",
            DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();
        CurrentAccountAuthorization authorization = new(context);

        await Assert.ThrowsAsync<AccountConflictException>(() =>
            authorization.ActivatePendingAccountAsync(
                identity,
                "other@example.com",
                identity.ToString(),
                CancellationToken.None));

        CurrentAccount current = await authorization.FindByIdentityAsync(identity, CancellationToken.None)
            ?? throw new InvalidOperationException("The invited account was not found.");
        Assert.Equal("ConvitePendente", current.Status);
    }

    [Fact]
    public async Task FindByIdentity_ReturnsCurrentPersistedRoleAndStatus()
    {
        await using SistemaEstoqueDbContext context = CreateContext(Guid.NewGuid().ToString());
        Guid identity = Guid.NewGuid();
        ContaDeUsuario account = ContaDeUsuario.CriarConvite(
            identity,
            "operator@example.com",
            "Operator",
            PerfilUsuario.Operador,
            "admin",
            DateTimeOffset.UtcNow);
        account.Ativar(DateTimeOffset.UtcNow, "user");
        context.ContasDeUsuario.Add(account);
        await context.SaveChangesAsync();
        CurrentAccountAuthorization authorization = new(context);

        CurrentAccount current = await authorization.FindByIdentityAsync(identity, CancellationToken.None)
            ?? throw new InvalidOperationException("The account was not found.");

        Assert.Equal("Operador", current.Role);
        Assert.Equal("Ativa", current.Status);
    }

    private static SistemaEstoqueDbContext CreateContext(string databaseName)
    {
        DbContextOptions<SistemaEstoqueDbContext> options = new DbContextOptionsBuilder<SistemaEstoqueDbContext>()
            .UseInMemoryDatabase(databaseName)
            .ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new SistemaEstoqueDbContext(options);
    }
}
