using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SistemaEstoque.Infrastructure.Persistence;

public sealed class PendingMigrationsStartupCheck(
    IServiceScopeFactory scopeFactory,
    ILogger<PendingMigrationsStartupCheck> logger) : IHostedService
{
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(5);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CheckTimeout);
        try
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            IPendingMigrationsProvider provider = scope.ServiceProvider.GetRequiredService<IPendingMigrationsProvider>();
            IReadOnlyList<string> pending = await provider.GetPendingMigrationsAsync(timeout.Token);
            if (pending.Count > 0)
            {
                logger.LogWarning(
                    "Database has pending migrations: {PendingMigrations}. Apply them with 'dotnet ef database update' before using the API.",
                    string.Join(", ", pending));
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Could not verify pending database migrations at startup.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
