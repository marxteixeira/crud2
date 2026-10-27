using Microsoft.EntityFrameworkCore;
using SistemaEstoque.Application.Products;
using SistemaEstoque.Domain.Entities;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.API.Tests;

public sealed class ProductSearchBySupplierTests
{
    [Fact]
    public async Task ListAsync_FiltersByActiveSupplierLinkWithoutDuplicatingProducts()
    {
        DbContextOptions<SistemaEstoqueDbContext> options = new DbContextOptionsBuilder<SistemaEstoqueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using SistemaEstoqueDbContext context = new(options);
        Guid actor = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Fornecedor supplier = Fornecedor.Criar("Fornecedor A", "52998224725", null, null, null, null, actor, now);
        Fornecedor anotherSupplier = Fornecedor.Criar("Fornecedor B", "04252011000110", null, null, null, null, actor, now);
        Estante rack = Estante.Criar("A", null, actor, now);
        Prateleira shelf = Prateleira.Criar(rack.Id, "1", actor, now);
        Produto first = Produto.Criar("Produto compartilhado A", 1, 1m, shelf.Id, SituacaoCadastro.Ativa, actor, now);
        Produto second = Produto.Criar("Produto compartilhado B", 1, 1m, shelf.Id, SituacaoCadastro.Ativa, actor, now);
        Produto historical = Produto.Criar("Produto histórico", 1, null, null, SituacaoCadastro.Inativa, actor, now);
        context.AddRange(supplier, anotherSupplier, rack, shelf, first, second, historical);
        context.VinculosProdutoFornecedor.AddRange(
            VinculoProdutoFornecedor.Criar(first.Id, supplier.Id, actor, now),
            VinculoProdutoFornecedor.Criar(first.Id, anotherSupplier.Id, actor, now),
            VinculoProdutoFornecedor.Criar(second.Id, supplier.Id, actor, now),
            VinculoProdutoFornecedor.Criar(second.Id, anotherSupplier.Id, actor, now),
            CreateInactiveLink(historical, supplier, actor, now));
        await context.SaveChangesAsync();

        ProductManagementService service = new(context);
        PaginatedProductsDto result = await service.ListAsync(
            "compartilhado", rack.Id, shelf.Id, "Active", supplier.Id, 2, 1, CancellationToken.None);

        ProductDto match = Assert.Single(result.Items);
        Assert.Equal(second.Id, match.Id);
        Assert.Equal("Produto compartilhado B", match.Name);
        Assert.Equal(2, result.Total);

        PaginatedProductsDto missingSupplierResult = await service.ListAsync(
            null, null, null, null, Guid.NewGuid(), 1, 25, CancellationToken.None);
        Assert.Empty(missingSupplierResult.Items);
    }

    private static VinculoProdutoFornecedor CreateInactiveLink(
        Produto product,
        Fornecedor supplier,
        Guid actor,
        DateTimeOffset now)
    {
        VinculoProdutoFornecedor link = VinculoProdutoFornecedor.Criar(product.Id, supplier.Id, actor, now);
        link.DefinirSituacao(SituacaoCadastro.Inativa, actor, now.AddMinutes(1));
        return link;
    }
}
