using Microsoft.EntityFrameworkCore;

namespace SistemaEstoque.Infrastructure.Persistence;

public interface IPendingMigrationsProvider
{
    Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken);
}

public sealed class EfPendingMigrationsProvider(SistemaEstoqueDbContext dbContext) : IPendingMigrationsProvider
{
    public async Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken) =>
        (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
}
