using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using SistemaEstoque.Application.Authentication;
using SistemaEstoque.Application.Locations;
using SistemaEstoque.Application.Products;
using SistemaEstoque.Application.Users;
using SistemaEstoque.Infrastructure.Identity;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool requireVerifiedTls = false)
    {
        string? connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Required configuration 'ConnectionStrings:Default' is missing.");
        }

        NpgsqlConnectionStringBuilder connectionOptions;
        try
        {
            connectionOptions = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException)
        {
            throw new InvalidOperationException(
                "Configuration 'ConnectionStrings:Default' is invalid.");
        }

        if (requireVerifiedTls
            && (connectionOptions.SslMode != SslMode.VerifyFull
                || connectionOptions.TrustServerCertificate))
        {
            throw new InvalidOperationException(
                "Configuration 'ConnectionStrings:Default' must require verified TLS.");
        }

        services.AddDbContext<SistemaEstoqueDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<ICurrentAccountAuthorization, CurrentAccountAuthorization>();
        services.AddHttpClient<SupabaseAdminClient>();
        services.AddScoped<IIdentityAdministration>(provider =>
            provider.GetRequiredService<SupabaseAdminClient>());
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<ILocationManagementService, LocationManagementService>();
        services.AddScoped<IProductManagementService, ProductManagementService>();

        services.AddHealthChecks()
            .AddCheck<DatabaseReadinessHealthCheck>("postgresql", tags: ["ready"]);

        return services;
    }
}
