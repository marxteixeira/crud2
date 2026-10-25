using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.API.Tests;

public sealed class RequiresLocalDatabaseFactAttribute : FactAttribute
{
    public const string VariableName = "ConnectionStrings__Default";

    public RequiresLocalDatabaseFactAttribute()
    {
        string? value = Environment.GetEnvironmentVariable(VariableName);
        if (string.IsNullOrWhiteSpace(value))
        {
            Skip = $"Defina {VariableName} apontando para um PostgreSQL descartável em localhost (veja o quickstart da 004).";
            return;
        }

        string host = new NpgsqlConnectionStringBuilder(value).Host ?? string.Empty;
        if (host != "localhost" && host != "127.0.0.1")
        {
            Skip = $"{VariableName} deve apontar para localhost; o teste nunca usa bancos remotos.";
        }
    }
}

public sealed class PersistenceConnectivityTests
{
    [RequiresLocalDatabaseFact]
    public async Task DbContext_CanConnectToConfiguredPostgreSql()
    {
        string connectionString = Environment.GetEnvironmentVariable(RequiresLocalDatabaseFactAttribute.VariableName)!;
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = connectionString
                })));
        using IServiceScope scope = factory.Services.CreateScope();
        SistemaEstoqueDbContext dbContext = scope.ServiceProvider.GetRequiredService<SistemaEstoqueDbContext>();

        bool canConnect = await dbContext.Database.CanConnectAsync();

        Assert.True(canConnect);
    }
}

