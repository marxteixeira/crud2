using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using SistemaEstoque.Application.Users;
using SistemaEstoque.Domain.Entities;
using SistemaEstoque.Infrastructure.Identity;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.API.Tests;

public sealed class UserManagementServiceTests
{
    [Fact]
    public async Task Invite_RejectsExistingActiveOrPendingEmailBeforeProviderCall()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        ContaDeUsuario existing = CreateAccount("existing@example.com");
        existing.Atualizar("reserved@example.com", null, null, "admin", DateTimeOffset.UtcNow);
        context.ContasDeUsuario.Add(existing);
        await context.SaveChangesAsync();
        FakeIdentityAdministration identity = new();
        UserManagementService service = CreateService(context, identity);

        await Assert.ThrowsAsync<AccountConflictException>(() => service.InviteAsync(
            new CreateUserRequest("RESERVED@example.com", "New User", "Operador"),
            "admin",
            CancellationToken.None));

        Assert.Equal(0, identity.InviteCalls);
    }

    [Fact]
    public async Task ResendInvitation_PendingAccountCallsProvider()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        ContaDeUsuario pending = CreateAccount("pending@example.com");
        context.ContasDeUsuario.Add(pending);
        await context.SaveChangesAsync();
        FakeIdentityAdministration identity = new();
        UserManagementService service = CreateService(context, identity);

        await service.ResendInvitationAsync(pending.Id, CancellationToken.None);

        Assert.Equal(1, identity.InviteCalls);
    }

    [Fact]
    public async Task ResendInvitation_ActiveAccountIsConflict()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        ContaDeUsuario active = CreateAccount("active@example.com");
        active.DefinirSituacao(SituacaoConta.Ativa, DateTimeOffset.UtcNow, "admin");
        context.ContasDeUsuario.Add(active);
        await context.SaveChangesAsync();
        FakeIdentityAdministration identity = new();
        UserManagementService service = CreateService(context, identity);

        await Assert.ThrowsAsync<AccountConflictException>(
            () => service.ResendInvitationAsync(active.Id, CancellationToken.None));

        Assert.Equal(0, identity.InviteCalls);
    }

    [Fact]
    public async Task ResendInvitation_UnknownAccountIsNotFound()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        UserManagementService service = CreateService(context, new FakeIdentityAdministration());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ResendInvitationAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Invite_CreatesPendingAccountAndSynchronizesProviderRole()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        FakeIdentityAdministration identity = new();
        UserManagementService service = CreateService(context, identity);

        UserAccountDto created = await service.InviteAsync(
            new CreateUserRequest("NEW@example.com", "New User", "Operador"),
            "admin-account",
            CancellationToken.None);
        ContaDeUsuario persisted = await context.ContasDeUsuario.SingleAsync();

        Assert.Equal("new@example.com", created.Email);
        Assert.Equal("ConvitePendente", created.Status);
        Assert.Equal(1, identity.InviteCalls);
        Assert.Equal("Operador", identity.LastRole);
        Assert.Equal("admin-account", persisted.CriadoPor);
        Assert.Equal("admin-account", persisted.AtualizadoPor);
    }

    [Fact]
    public async Task Invite_PersistenceFailureDeletesProviderIdentity()
    {
        await using SistemaEstoqueDbContext context = CreateContext(failSaveChanges: true);
        FakeIdentityAdministration identity = new();
        UserManagementService service = CreateService(context, identity);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.InviteAsync(
                new CreateUserRequest("user@example.com", "New User", "Operador"),
                "admin",
                CancellationToken.None));

        Assert.Equal(1, identity.InviteCalls);
        Assert.Equal(1, identity.DeleteCalls);
        Assert.Contains("não foi persistida", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Invite_ReportsWhenProviderCompensationAlsoFails()
    {
        await using SistemaEstoqueDbContext context = CreateContext(failSaveChanges: true);
        FakeIdentityAdministration identity = new() { FailDelete = true };
        UserManagementService service = CreateService(context, identity);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.InviteAsync(
                new CreateUserRequest("user@example.com", "New User", "Operador"),
                "admin",
                CancellationToken.None));

        Assert.Contains("compensada", exception.Message, StringComparison.Ordinal);
        Assert.Equal(1, identity.DeleteCalls);
    }

    [Fact]
    public async Task SetStatus_RefusesToDeactivateLastActiveAdministrator()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        ContaDeUsuario admin = CreateAccount("admin@example.com", PerfilUsuario.Administrador);
        admin.Ativar(DateTimeOffset.UtcNow, "bootstrap");
        context.ContasDeUsuario.Add(admin);
        await context.SaveChangesAsync();
        UserManagementService service = CreateService(context, new FakeIdentityAdministration());

        await Assert.ThrowsAsync<AccountConflictException>(() => service.SetStatusAsync(
            admin.Id,
            SituacaoConta.Inativa,
            "admin",
            CancellationToken.None));

        Assert.Equal(SituacaoConta.Ativa, (await context.ContasDeUsuario.SingleAsync()).Situacao);
    }

    [Fact]
    public async Task SetStatus_DoesNotActivateInviteBeforeHolderConfirmsIt()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        ContaDeUsuario account = CreateAccount("pending@example.com");
        context.ContasDeUsuario.Add(account);
        await context.SaveChangesAsync();
        UserManagementService service = CreateService(context, new FakeIdentityAdministration());

        await Assert.ThrowsAsync<AccountConflictException>(() => service.SetStatusAsync(
            account.Id,
            SituacaoConta.Ativa,
            "admin",
            CancellationToken.None));

        Assert.Equal(SituacaoConta.ConvitePendente, (await context.ContasDeUsuario.SingleAsync()).Situacao);
    }

    [Fact]
    public async Task Update_RequiresAnotherActiveAdministratorBeforeDemotion()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        ContaDeUsuario admin = CreateAccount("admin@example.com", PerfilUsuario.Administrador);
        admin.Ativar(DateTimeOffset.UtcNow, "bootstrap");
        context.ContasDeUsuario.Add(admin);
        await context.SaveChangesAsync();
        UserManagementService service = CreateService(context, new FakeIdentityAdministration());

        await Assert.ThrowsAsync<AccountConflictException>(() => service.UpdateAsync(
            admin.Id,
            new UpdateUserRequest(null, null, "Operador"),
            "admin",
            CancellationToken.None));

        Assert.Equal(PerfilUsuario.Administrador, (await context.ContasDeUsuario.SingleAsync()).Perfil);
    }

    [Fact]
    public async Task Update_RecordsAuditAndKeepsNewEmailPendingConfirmation()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        ContaDeUsuario operatorAccount = CreateAccount("operator@example.com");
        operatorAccount.Ativar(DateTimeOffset.UtcNow, "bootstrap");
        ContaDeUsuario admin = CreateAccount("admin@example.com", PerfilUsuario.Administrador);
        admin.Ativar(DateTimeOffset.UtcNow, "bootstrap");
        context.ContasDeUsuario.AddRange(operatorAccount, admin);
        await context.SaveChangesAsync();
        FakeIdentityAdministration identity = new();
        UserManagementService service = CreateService(context, identity);

        UserAccountDto updated = await service.UpdateAsync(
            operatorAccount.Id,
            new UpdateUserRequest("new@example.com", "Updated User", "Administrador"),
            "admin-actor",
            CancellationToken.None);
        ContaDeUsuario persisted = await context.ContasDeUsuario.SingleAsync(account => account.Id == operatorAccount.Id);

        Assert.Equal("operator@example.com", updated.Email);
        Assert.Equal("new@example.com", updated.PendingEmail);
        Assert.Equal("Administrador", updated.Role);
        Assert.Equal("admin-actor", persisted.AtualizadoPor);
        Assert.Equal("new@example.com", identity.RequestedEmail);
        Assert.Equal("Administrador", identity.LastRole);
    }

    [Fact]
    public async Task ConfirmEmail_UpdatesActiveAddressOnlyForMatchingPendingEmail()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        Guid identity = Guid.NewGuid();
        ContaDeUsuario account = ContaDeUsuario.CriarConvite(
            identity,
            "current@example.com",
            "Test User",
            PerfilUsuario.Operador,
            "bootstrap",
            DateTimeOffset.UtcNow);
        account.Atualizar("new@example.com", null, null, "admin", DateTimeOffset.UtcNow);
        context.ContasDeUsuario.Add(account);
        await context.SaveChangesAsync();
        UserManagementService service = CreateService(context, new FakeIdentityAdministration());

        await service.ConfirmEmailAsync(identity, "NEW@example.com", CancellationToken.None);
        ContaDeUsuario updated = await context.ContasDeUsuario.SingleAsync();

        Assert.Equal("new@example.com", updated.Email);
        Assert.Null(updated.EmailPendente);
        Assert.Equal(identity.ToString(), updated.AtualizadoPor);
    }

    private static SistemaEstoqueDbContext CreateContext(bool failSaveChanges = false)
    {
        DbContextOptionsBuilder<SistemaEstoqueDbContext> optionsBuilder =
            new DbContextOptionsBuilder<SistemaEstoqueDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        if (failSaveChanges)
        {
            optionsBuilder.AddInterceptors(new FailingSaveChangesInterceptor());
        }

        DbContextOptions<SistemaEstoqueDbContext> options = optionsBuilder.Options;
        return new SistemaEstoqueDbContext(options);
    }

    private static ContaDeUsuario CreateAccount(
        string email,
        PerfilUsuario role = PerfilUsuario.Operador) =>
        ContaDeUsuario.CriarConvite(
            Guid.NewGuid(),
            email,
            "Test User",
            role,
            "bootstrap",
            DateTimeOffset.UtcNow);

    private static UserManagementService CreateService(
        SistemaEstoqueDbContext context,
        FakeIdentityAdministration identity) =>
        new(
            context,
            identity,
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator(),
            NullLogger<UserManagementService>.Instance);

    private sealed class FakeIdentityAdministration : IIdentityAdministration
    {
        public int InviteCalls { get; private set; }

        public int DeleteCalls { get; private set; }

        public bool FailDelete { get; init; }

        public string? LastRole { get; private set; }

        public string? RequestedEmail { get; private set; }

        public Task<Guid> InviteAsync(string email, string displayName, CancellationToken cancellationToken)
        {
            InviteCalls++;
            return Task.FromResult(Guid.NewGuid());
        }

        public Task DeleteAsync(Guid identity, CancellationToken cancellationToken)
        {
            DeleteCalls++;
            if (FailDelete)
            {
                throw new HttpRequestException("provider failure");
            }

            return Task.CompletedTask;
        }

        public Task UpdateRoleAsync(Guid identity, string role, CancellationToken cancellationToken)
        {
            LastRole = role;
            return Task.CompletedTask;
        }

        public Task RequestEmailChangeAsync(Guid identity, string email, CancellationToken cancellationToken)
        {
            RequestedEmail = email;
            return Task.CompletedTask;
        }

        public Task<string?> GetConfirmedEmailAsync(string accessToken, CancellationToken cancellationToken) =>
            Task.FromResult<string?>(null);
    }

    private sealed class FailingSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromException<InterceptionResult<int>>(new DbUpdateException("Test persistence failure."));
    }
}
