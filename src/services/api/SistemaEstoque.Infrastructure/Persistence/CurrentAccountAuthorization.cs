using Microsoft.EntityFrameworkCore;
using SistemaEstoque.Application.Authentication;
using SistemaEstoque.Application.Users;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Infrastructure.Persistence;

public sealed class CurrentAccountAuthorization(SistemaEstoqueDbContext dbContext)
    : ICurrentAccountAuthorization
{
    public async Task<CurrentAccount?> FindByIdentityAsync(
        Guid identity,
        CancellationToken cancellationToken)
    {
        return await dbContext.ContasDeUsuario
            .AsNoTracking()
            .Where(account => account.Identidade == identity)
            .Select(account => new CurrentAccount(
                account.Id,
                account.Identidade,
                account.Email,
                account.NomeExibicao,
                account.Perfil.ToString(),
                account.Situacao.ToString()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<bool> IsSessionRevokedAsync(Guid sessionId, CancellationToken cancellationToken) =>
        dbContext.SessoesRevogadas.AsNoTracking()
            .AnyAsync(session => session.Identificador == sessionId, cancellationToken);

    public async Task ActivatePendingAccountAsync(
        Guid identity,
        string confirmedEmail,
        string actor,
        CancellationToken cancellationToken)
    {
        ContaDeUsuario account = await dbContext.ContasDeUsuario
            .SingleOrDefaultAsync(item => item.Identidade == identity, cancellationToken)
            ?? throw new KeyNotFoundException("A conta vinculada à identidade não foi encontrada.");
        if (!string.Equals(
                account.Email,
                confirmedEmail.Trim().ToLowerInvariant(),
                StringComparison.Ordinal))
        {
            throw new AccountConflictException("O e-mail confirmado não corresponde ao convite.");
        }

        account.Ativar(DateTimeOffset.UtcNow, actor);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        if (!await IsSessionRevokedAsync(sessionId, cancellationToken))
        {
            SessaoRevogada revokedSession = new(sessionId, DateTimeOffset.UtcNow);
            dbContext.SessoesRevogadas.Add(revokedSession);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                dbContext.Entry(revokedSession).State = EntityState.Detached;
                if (!await IsSessionRevokedAsync(sessionId, cancellationToken))
                {
                    throw;
                }
            }
        }
    }
}
