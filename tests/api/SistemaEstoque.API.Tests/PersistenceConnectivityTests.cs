using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        bool canConnect = await dbContext.Database.CanConnectAsync();

        Assert.True(canConnect);
    }
}
