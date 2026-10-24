using System.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaEstoque.Application.Users;
using SistemaEstoque.Domain.Entities;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.Infrastructure.Identity;

public sealed class UserManagementService(
    SistemaEstoqueDbContext dbContext,
    IIdentityAdministration identityAdministration,
    IValidator<CreateUserRequest> createValidator,
    IValidator<UpdateUserRequest> updateValidator,
    ILogger<UserManagementService> logger) : IUserManagementService
{
    public async Task<PaginatedUsersDto> ListAsync(
        string? search,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<ContaDeUsuario> query = dbContext.ContasDeUsuario.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            string normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(account =>
                account.Email.Contains(normalizedSearch)
                || account.NomeExibicao.ToLower().Contains(normalizedSearch));
        }

        if (status is not null && Enum.TryParse(status, false, out SituacaoConta parsedStatus))
        {
            query = query.Where(account => account.Situacao == parsedStatus);
        }

        int total = await query.CountAsync(cancellationToken);
        List<UserAccountDto> items = await query
            .OrderBy(account => account.NomeExibicao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDto())
            .ToListAsync(cancellationToken);
        return new PaginatedUsersDto(items, page, pageSize, total);
    }

    public async Task<UserAccountDto?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.ContasDeUsuario.AsNoTracking()
            .Where(account => account.Id == id)
            .Select(ToDto())
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<UserAccountDto> InviteAsync(
        CreateUserRequest request,
        string actor,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        string email = request.Email.Trim().ToLowerInvariant();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        if (await EmailInUseAsync(email, cancellationToken))
        {
            throw new AccountConflictException("O e-mail já pertence a uma conta.");
        }

        PerfilUsuario role = Enum.Parse<PerfilUsuario>(request.Role, ignoreCase: false);
        Guid identity = await identityAdministration.InviteAsync(email, request.DisplayName.Trim(), cancellationToken);
        ContaDeUsuario account = ContaDeUsuario.CriarConvite(
            identity,
            email,
            request.DisplayName,
            role,
            actor,
            DateTimeOffset.UtcNow);
        dbContext.ContasDeUsuario.Add(account);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception persistenceException) when (
            persistenceException is DbUpdateException or PostgresException)
        {
            Exception? rollbackException = null;
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch (Exception exception) when (exception is NpgsqlException or InvalidOperationException)
            {
                rollbackException = exception;
            }

            try
            {
                await identityAdministration.DeleteAsync(identity, CancellationToken.None);
            }
            catch (HttpRequestException compensationException)
            {
                logger.LogError(
                    "Invite compensation failed; PersistenceError {PersistenceType}; ProviderError {ProviderType}; RollbackError {RollbackType}",
                    persistenceException.GetType().Name,
                    compensationException.GetType().Name,
                    rollbackException?.GetType().Name);
                throw new InvalidOperationException(
                    "A conta não foi persistida e a identidade do provedor não pôde ser compensada.",
                    new AggregateException(
                        new[] { persistenceException, compensationException, rollbackException }
                            .OfType<Exception>()));
            }

            if (rollbackException is not null)
            {
                throw new InvalidOperationException(
                    "A persistência da conta falhou e a transação não pôde ser confirmada como revertida.",
                    new AggregateException(persistenceException, rollbackException));
            }

            if (IsDatabaseConflict(persistenceException))
            {
                throw new AccountConflictException("O e-mail já pertence a uma conta.");
            }

            throw new InvalidOperationException(
                "A conta não foi persistida; a identidade de convite do provedor foi removida.",
                persistenceException);
        }

        try
        {
            await identityAdministration.UpdateRoleAsync(identity, request.Role, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                "Provider role synchronization failed; AccountId {AccountId}; ErrorType {ErrorType}",
                account.Id,
                exception.GetType().Name);
            throw new InvalidOperationException(
                "A conta foi criada, mas a sincronização do provedor falhou e requer reconciliação.",
                exception);
        }

        return ToDto(account);
    }

    private static bool IsDatabaseConflict(Exception exception)
    {
        PostgresException? postgresException = exception switch
        {
            PostgresException direct => direct,
            DbUpdateException { InnerException: PostgresException inner } => inner,
            _ => null
        };
        return postgresException?.SqlState is PostgresErrorCodes.UniqueViolation
            or PostgresErrorCodes.SerializationFailure;
    }

    public async Task<UserAccountDto> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        string actor,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        ContaDeUsuario account = await dbContext.ContasDeUsuario
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("A conta solicitada não foi encontrada.");

        string? newEmail = request.Email?.Trim().ToLowerInvariant();
        if (newEmail is not null && await EmailInUseAsync(newEmail, cancellationToken, id))
        {
            throw new AccountConflictException("O e-mail já pertence a outra conta.");
        }

        PerfilUsuario? newRole = request.Role is null
            ? null
            : Enum.Parse<PerfilUsuario>(request.Role, ignoreCase: false);
        if (account.Situacao == SituacaoConta.Ativa
            && account.Perfil == PerfilUsuario.Administrador
            && newRole is PerfilUsuario.Operador)
        {
            await EnsureAnotherAdministratorAsync(id, cancellationToken);
        }

        account.Atualizar(newEmail, request.DisplayName, newRole, actor, DateTimeOffset.UtcNow);
        bool providerChanged = false;
        try
        {
            if (newEmail is not null)
            {
                await identityAdministration.RequestEmailChangeAsync(account.Identidade, newEmail, cancellationToken);
                providerChanged = true;
            }

            if (newRole.HasValue)
            {
                await identityAdministration.UpdateRoleAsync(account.Identidade, newRole.Value.ToString(), cancellationToken);
                providerChanged = true;
            }
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                "User update provider synchronization failed; AccountId {AccountId}; ErrorType {ErrorType}; ReconciliationRequired {ReconciliationRequired}",
                account.Id,
                exception.GetType().Name,
                true);
            throw new InvalidOperationException(
                "A sincronização do provedor falhou; verifique a conta antes de repetir a operação.",
                exception);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (
            providerChanged
            && exception is not OperationCanceledException)
        {
            logger.LogError(
                "User update persistence failed after provider synchronization; AccountId {AccountId}; ErrorType {ErrorType}; ReconciliationRequired {ReconciliationRequired}",
                account.Id,
                exception.GetType().Name,
                true);
            throw new InvalidOperationException(
                "A sincronização do provedor foi concluída, mas a persistência falhou; a conta requer reconciliação.",
                exception);
        }

        return ToDto(account);
    }

    public async Task SetStatusAsync(
        Guid id,
        SituacaoConta status,
        string actor,
        CancellationToken cancellationToken)
    {
        if (status is not (SituacaoConta.Ativa or SituacaoConta.Inativa))
        {
            throw new ValidationException("Situação deve ser Ativa ou Inativa.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        ContaDeUsuario account = await dbContext.ContasDeUsuario
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("A conta solicitada não foi encontrada.");
        if (account.Situacao == SituacaoConta.ConvitePendente)
        {
            throw new AccountConflictException("Somente o titular pode ativar uma conta com convite pendente.");
        }

        if (status == SituacaoConta.Inativa
            && account.Situacao == SituacaoConta.Ativa
            && account.Perfil == PerfilUsuario.Administrador)
        {
            await EnsureAnotherAdministratorAsync(id, cancellationToken);
        }

        account.DefinirSituacao(status, DateTimeOffset.UtcNow, actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task ConfirmEmailAsync(
        Guid identity,
        string confirmedEmail,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = confirmedEmail.Trim().ToLowerInvariant();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        ContaDeUsuario account = await dbContext.ContasDeUsuario
            .SingleOrDefaultAsync(item => item.Identidade == identity, cancellationToken)
            ?? throw new KeyNotFoundException("A conta vinculada à identidade não foi encontrada.");
        if (await EmailInUseAsync(normalizedEmail, cancellationToken, account.Id))
        {
            throw new AccountConflictException("O e-mail já pertence a outra conta.");
        }
        if (!string.Equals(account.EmailPendente, normalizedEmail, StringComparison.Ordinal))
        {
            throw new AccountConflictException("O e-mail confirmado não corresponde à troca pendente.");
        }

        account.ConfirmarEmail(normalizedEmail, DateTimeOffset.UtcNow, identity.ToString());
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<bool> EmailInUseAsync(
        string email,
        CancellationToken cancellationToken,
        Guid? exceptId = null) =>
        await dbContext.ContasDeUsuario.AnyAsync(account =>
            account.Id != exceptId
            && (account.Email == email || account.EmailPendente == email),
            cancellationToken);

    private async Task EnsureAnotherAdministratorAsync(Guid excludedId, CancellationToken cancellationToken)
    {
        bool anotherAdministratorExists = await dbContext.ContasDeUsuario.AnyAsync(account =>
            account.Id != excludedId
            && account.Perfil == PerfilUsuario.Administrador
            && account.Situacao == SituacaoConta.Ativa,
            cancellationToken);
        if (!anotherAdministratorExists)
        {
            throw new AccountConflictException("A operação não pode remover o último Administrador ativo.");
        }
    }

    private static System.Linq.Expressions.Expression<Func<ContaDeUsuario, UserAccountDto>> ToDto() =>
        account => new UserAccountDto(
            account.Id,
            account.Email,
            account.NomeExibicao,
            account.EmailPendente,
            account.Perfil.ToString(),
            account.Situacao.ToString());

    private static UserAccountDto ToDto(ContaDeUsuario account) =>
        new(account.Id, account.Email, account.NomeExibicao, account.EmailPendente,
            account.Perfil.ToString(), account.Situacao.ToString());
}
