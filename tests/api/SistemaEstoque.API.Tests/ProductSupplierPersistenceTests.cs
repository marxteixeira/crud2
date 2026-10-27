using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SistemaEstoque.Application.Suppliers;
using SistemaEstoque.Domain.Entities;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.API.Tests;

public sealed class ProductSupplierPersistenceTests
{
    [Fact]
    public async Task AssociateAsync_ReactivatesHistoricalLinkAndPreservesCreationAudit()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        Guid creator = Guid.NewGuid();
        (Produto product, Fornecedor supplier) = await SeedActivePairAsync(context, creator);
        ProductSupplierService service = new(context);

        await service.AssociateAsync(product.Id, supplier.Id, creator, CancellationToken.None);
        VinculoProdutoFornecedor created = await context.VinculosProdutoFornecedor.SingleAsync();
        DateTimeOffset createdAt = created.CriadoEm;
        Fornecedor anotherSupplier = Fornecedor.Criar(
            "Outro fornecedor", "04252011000110", null, null, null, null, creator, DateTimeOffset.UtcNow);
        context.Fornecedores.Add(anotherSupplier);
        context.VinculosProdutoFornecedor.Add(
            VinculoProdutoFornecedor.Criar(product.Id, anotherSupplier.Id, creator, DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();
        Guid disconnector = Guid.NewGuid();

        await service.DisassociateAsync(product.Id, supplier.Id, disconnector, CancellationToken.None);
        Guid reactivator = Guid.NewGuid();
        await service.AssociateAsync(product.Id, supplier.Id, reactivator, CancellationToken.None);

        VinculoProdutoFornecedor reactivated = await context.VinculosProdutoFornecedor.SingleAsync(
            link => link.ProdutoId == product.Id && link.FornecedorId == supplier.Id);
        Assert.Equal(created.Id, reactivated.Id);
        Assert.Equal(createdAt, reactivated.CriadoEm);
        Assert.Equal(creator, reactivated.CriadoPor);
        Assert.Equal(SituacaoCadastro.Ativa, reactivated.Situacao);
        Assert.Equal(reactivator, reactivated.AtualizadoPor);
        Assert.True(reactivated.AtualizadoEm > createdAt);

        PaginatedSupplierLinksDto activeLinks = (await service.ListSuppliersAsync(
            product.Id, null, 1, 25, CancellationToken.None))!;
        Assert.Equal(2, activeLinks.Items.Count);
        Assert.Contains(activeLinks.Items, link => link.SupplierId == supplier.Id);

        await service.DisassociateAsync(product.Id, supplier.Id, disconnector, CancellationToken.None);
        PaginatedSupplierLinksDto historicalLinks = (await service.ListSuppliersAsync(
            product.Id, "Inactive", 1, 25, CancellationToken.None))!;
        Assert.Equal("Inactive", Assert.Single(historicalLinks.Items).LinkStatus);
    }

    [Fact]
    public async Task AssociateAsync_RejectsInactiveProductsAndSuppliers()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        Guid actor = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        (Produto activeProduct, Fornecedor activeSupplier) = await SeedActivePairAsync(context, actor);
        Fornecedor inactiveSupplier = Fornecedor.Criar(
            "Fornecedor inativo", "04252011000110", null, null, null, null, actor, now);
        inactiveSupplier.DefinirSituacao(SituacaoCadastro.Inativa, actor, now.AddMinutes(1));
        Produto inactiveProduct = Produto.Criar(
            "Produto inativo", 1, null, null, SituacaoCadastro.Inativa, actor, now);
        context.AddRange(inactiveSupplier, inactiveProduct);
        await context.SaveChangesAsync();
        ProductSupplierService service = new(context);

        await Assert.ThrowsAsync<ProductSupplierConflictException>(() => service.AssociateAsync(
            inactiveProduct.Id, activeSupplier.Id, actor, CancellationToken.None));
        await Assert.ThrowsAsync<ProductSupplierConflictException>(() => service.AssociateAsync(
            activeProduct.Id, inactiveSupplier.Id, actor, CancellationToken.None));
        Assert.Empty(await context.VinculosProdutoFornecedor.ToListAsync());
    }

    private static async Task<(Produto Product, Fornecedor Supplier)> SeedActivePairAsync(
        SistemaEstoqueDbContext context,
        Guid actor)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Fornecedor supplier = Fornecedor.Criar(
            "Fornecedor ativo", "52998224725", null, null, null, null, actor, now);
        Estante rack = Estante.Criar("A", null, actor, now);
        Prateleira shelf = Prateleira.Criar(rack.Id, "1", actor, now);
        Produto product = Produto.Criar(
            "Produto ativo", 1, 1m, shelf.Id, SituacaoCadastro.Ativa, actor, now);
        context.AddRange(supplier, rack, shelf, product);
        await context.SaveChangesAsync();
        return (product, supplier);
    }

    private static SistemaEstoqueDbContext CreateContext()
    {
        DbContextOptions<SistemaEstoqueDbContext> options = new DbContextOptionsBuilder<SistemaEstoqueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new SistemaEstoqueDbContext(options);
    }
}
