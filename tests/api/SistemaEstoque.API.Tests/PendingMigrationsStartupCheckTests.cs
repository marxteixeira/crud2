using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.API.Tests;

public sealed class PendingMigrationsStartupCheckTests
{
    [Fact]
    public async Task StartAsync_WarnsWithPendingMigrationNames()
    {
        RecordingLogger logger = new();
        PendingMigrationsStartupCheck check = Create(new FakeProvider(["20261025180125_AddStorageLocations"]), logger);

        await check.StartAsync(CancellationToken.None);

        string message = Assert.Single(logger.Warnings);
        Assert.Contains("20261025180125_AddStorageLocations", message, StringComparison.Ordinal);
        Assert.Contains("dotnet ef database update", message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task StartAsync_DoesNotWarnWhenNothingIsPending()
    {
        RecordingLogger logger = new();
        PendingMigrationsStartupCheck check = Create(new FakeProvider([]), logger);

        await check.StartAsync(CancellationToken.None);

        Assert.Empty(logger.Warnings);
    }

    [Fact]
    public async Task StartAsync_DoesNotThrowWhenDatabaseIsUnavailable()
    {
        RecordingLogger logger = new();
        PendingMigrationsStartupCheck check = Create(new FakeProvider(null), logger);

        await check.StartAsync(CancellationToken.None);

        Assert.Single(logger.Warnings);
    }

    private static PendingMigrationsStartupCheck Create(IPendingMigrationsProvider provider, RecordingLogger logger)
    {
        ServiceProvider services = new ServiceCollection().AddSingleton(provider).BuildServiceProvider();
        return new PendingMigrationsStartupCheck(
            services.GetRequiredService<IServiceScopeFactory>(),
            logger);
    }

    private sealed class FakeProvider(IReadOnlyList<string>? pending) : IPendingMigrationsProvider
    {
        public Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken) =>
            pending is null
                ? throw new InvalidOperationException("database unavailable")
                : Task.FromResult(pending);
    }

    private sealed class RecordingLogger : ILogger<PendingMigrationsStartupCheck>
    {
        public List<string> Warnings { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
            {
                Warnings.Add(formatter(state, exception));
            }
        }
    }
}
