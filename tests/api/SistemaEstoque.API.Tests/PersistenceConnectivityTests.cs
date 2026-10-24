using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.API.Tests;

public sealed class PersistenceConnectivityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PersistenceConnectivityTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DbContext_CanConnectToConfiguredPostgreSql()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        SistemaEstoqueDbContext dbContext = scope.ServiceProvider.GetRequiredService<SistemaEstoqueDbContext>();
        NpgsqlConnectionStringBuilder connection =
            new(dbContext.Database.GetConnectionString() ?? string.Empty);

        Assert.Equal("localhost", connection.Host);
        Assert.Equal("crud2", connection.Database);
        bool canConnect = await dbContext.Database.CanConnectAsync();

        Assert.True(canConnect);
    }
}
