using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace SistemaEstoque.Infrastructure.Persistence;

public sealed class DatabaseReadinessHealthCheck(SistemaEstoqueDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("PostgreSQL is available.")
                : HealthCheckResult.Unhealthy("Required persistence is unavailable.");
        }
        catch (NpgsqlException)
        {
            return HealthCheckResult.Unhealthy("Required persistence is unavailable.");
        }
        catch (TimeoutException)
        {
            return HealthCheckResult.Unhealthy("Required persistence is unavailable.");
        }
    }
}
