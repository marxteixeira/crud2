using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Required configuration 'ConnectionStrings:Default' is missing.");
        }

        try
        {
            _ = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException)
        {
            throw new InvalidOperationException(
                "Configuration 'ConnectionStrings:Default' is invalid.");
        }

        services.AddDbContext<SistemaEstoqueDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHealthChecks()
            .AddCheck<DatabaseReadinessHealthCheck>("postgresql", tags: ["ready"]);

        return services;
    }
}
