using Microsoft.EntityFrameworkCore;
using SistemaEstoque.Application.Locations;
using SistemaEstoque.Application.Products;
using SistemaEstoque.Domain.Entities;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.API.Tests;

public sealed class LocationProductIntegrationTests
{
    [Fact]
    public async Task ShelfAndRackCannotBeInactivatedWhileTheyContainAnActiveProduct()
    {
        DbContextOptions<SistemaEstoqueDbContext> options = new DbContextOptionsBuilder<SistemaEstoqueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using SistemaEstoqueDbContext context = new(options);
        LocationManagementService locations = new(context);
        ProductManagementService products = new(context);
        Guid actor = Guid.NewGuid();
        RackDto rack = await locations.CreateRackAsync(new CreateRackRequest("A", null), actor, CancellationToken.None);
        ShelfDto shelf = await locations.CreateShelfAsync(
            rack.Id, new CreateShelfRequest("1"), actor, CancellationToken.None);
        await products.CreateAsync(
            new CreateProductRequest("Produto", 2, 4.50m, shelf.Id),
            actor,
            isAdministrator: true,
            CancellationToken.None);

        await Assert.ThrowsAsync<LocationConflictException>(() =>
            locations.SetShelfStatusAsync(
                rack.Id, shelf.Id, SituacaoCadastro.Inativa, actor, CancellationToken.None));
        await Assert.ThrowsAsync<LocationConflictException>(() =>
            locations.SetRackStatusAsync(
                rack.Id, SituacaoCadastro.Inativa, actor, CancellationToken.None));
    }

    [Fact]
    public async Task InactiveProductDoesNotPreventLocationInactivation()
    {
        DbContextOptions<SistemaEstoqueDbContext> options = new DbContextOptionsBuilder<SistemaEstoqueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using SistemaEstoqueDbContext context = new(options);
        LocationManagementService locations = new(context);
        Guid actor = Guid.NewGuid();
        RackDto rack = await locations.CreateRackAsync(new CreateRackRequest("B", null), actor, CancellationToken.None);
        ShelfDto shelf = await locations.CreateShelfAsync(
            rack.Id, new CreateShelfRequest("2"), actor, CancellationToken.None);
        context.Produtos.Add(Produto.Criar(
            "Produto inativo", 0, null, shelf.Id, SituacaoCadastro.Inativa, actor, DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();

        await locations.SetShelfStatusAsync(
            rack.Id, shelf.Id, SituacaoCadastro.Inativa, actor, CancellationToken.None);

        Assert.Equal("Inactive", (await locations.ListShelvesAsync(
            rack.Id, null, CancellationToken.None))?.Single().Status);
    }
}
