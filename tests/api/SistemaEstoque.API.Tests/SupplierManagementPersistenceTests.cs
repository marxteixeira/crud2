using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SistemaEstoque.Application.Suppliers;
using SistemaEstoque.Domain.Entities;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.API.Tests;

public sealed class SupplierManagementPersistenceTests
{
    [Fact]
    public async Task CreateAsync_NormalizesDocumentAndRejectsDuplicatesWithDifferentFormatting()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        SupplierManagementService service = new(context);
        Guid actor = Guid.NewGuid();

        SupplierDto created = await service.CreateAsync(
            new CreateSupplierRequest("Fornecedor A", "52998224725", null, null, null, null),
            actor,
            CancellationToken.None);

        Assert.Equal("52998224725", created.TaxIdentifier);
        Assert.Equal(actor, created.CreatedBy);
        Guid updater = Guid.NewGuid();
        SupplierDto updated = (await service.UpdateAsync(
            created.Id,
            new UpdateSupplierRequest("Fornecedor atualizado", "529.982.247-25", null, "", null, null),
            updater,
            CancellationToken.None))!;
        Assert.Equal("Fornecedor atualizado", updated.Name);
        Assert.Null(updated.Email);
        Assert.Equal(updater, updated.UpdatedBy);
        await Assert.ThrowsAsync<SupplierConflictException>(() => service.CreateAsync(
            new CreateSupplierRequest("Fornecedor B", "529.982.247-25", null, null, null, null),
            Guid.NewGuid(),
            CancellationToken.None));
    }

    [Fact]
    public async Task SetStatusAsync_RejectsInactivationWhenActiveProductIsLinked()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        Guid actor = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        (Fornecedor supplier, Produto product) = await SeedLinkedSupplierAsync(context, actor, now, activeProduct: true);
        SupplierManagementService service = new(context);

        await Assert.ThrowsAsync<SupplierConflictException>(() => service.SetStatusAsync(
            supplier.Id,
            SituacaoCadastro.Inativa,
            Guid.NewGuid(),
            CancellationToken.None));

        Assert.Equal(SituacaoCadastro.Ativa, (await context.Fornecedores.FindAsync(supplier.Id))!.Situacao);
        Assert.Equal(SituacaoCadastro.Ativa, (await context.Produtos.FindAsync(product.Id))!.Situacao);
    }

    [Fact]
    public async Task SetStatusAsync_AllowsInactivationWhenOnlyInactiveProductsAreLinked()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        Guid actor = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Fornecedor supplier = (await SeedLinkedSupplierAsync(context, actor, now, activeProduct: false)).Supplier;
        SupplierManagementService service = new(context);
        Guid updater = Guid.NewGuid();

        await service.SetStatusAsync(
            supplier.Id,
            SituacaoCadastro.Inativa,
            updater,
            CancellationToken.None);

        Fornecedor updated = (await context.Fornecedores.FindAsync(supplier.Id))!;
        Assert.Equal(SituacaoCadastro.Inativa, updated.Situacao);
        Assert.Equal(updater, updated.AtualizadoPor);
        Assert.True(updated.AtualizadoEm >= now);
    }

    [Fact]
    public async Task ListAsync_ReturnsTheRequestedPageAndTotal()
    {
        await using SistemaEstoqueDbContext context = CreateContext();
        Guid actor = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        context.Fornecedores.AddRange(
            Fornecedor.Criar("Fornecedor A", "52998224725", null, null, null, null, actor, now),
            Fornecedor.Criar("Fornecedor B", "04252011000110", null, null, null, null, actor, now),
            Fornecedor.Criar("Fornecedor C", "11144477735", null, null, null, null, actor, now));
        await context.SaveChangesAsync();
        SupplierManagementService service = new(context);

        PaginatedSuppliersDto result = await service.ListAsync(
            null, null, 2, 1, CancellationToken.None);

        Assert.Equal(3, result.Total);
        Assert.Equal(2, result.Page);
        Assert.Equal("Fornecedor B", Assert.Single(result.Items).Name);
    }

    private static async Task<(Fornecedor Supplier, Produto Product)> SeedLinkedSupplierAsync(
        SistemaEstoqueDbContext context,
        Guid actor,
        DateTimeOffset now,
        bool activeProduct)
    {
        Fornecedor supplier = Fornecedor.Criar(
            "Fornecedor vinculado", "04252011000110", null, null, null, null, actor, now);
        Estante rack = Estante.Criar("A", null, actor, now);
        Prateleira shelf = Prateleira.Criar(rack.Id, "1", actor, now);
        Produto product = Produto.Criar(
            "Produto vinculado",
            1,
            activeProduct ? 1m : null,
            activeProduct ? shelf.Id : null,
            activeProduct ? SituacaoCadastro.Ativa : SituacaoCadastro.Inativa,
            actor,
            now);
        context.AddRange(supplier, rack, shelf, product);
        context.VinculosProdutoFornecedor.Add(
            VinculoProdutoFornecedor.Criar(product.Id, supplier.Id, actor, now));
        await context.SaveChangesAsync();
        return (supplier, product);
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
