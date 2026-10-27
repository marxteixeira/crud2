using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SistemaEstoque.Application.Suppliers;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Infrastructure.Persistence;

public sealed class ProductSupplierService(SistemaEstoqueDbContext dbContext) : IProductSupplierService
{
    public async Task<PaginatedSupplierLinksDto?> ListSuppliersAsync(
        Guid productId,
        string? linkStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        bool productExists = await dbContext.Produtos.AsNoTracking()
            .AnyAsync(product => product.Id == productId, cancellationToken);
        if (!productExists)
        {
            return null;
        }

        IQueryable<VinculoProdutoFornecedor> query = dbContext.VinculosProdutoFornecedor
            .AsNoTracking()
            .Where(link => link.ProdutoId == productId);
        SituacaoCadastro status = linkStatus == "Inactive"
            ? SituacaoCadastro.Inativa
            : SituacaoCadastro.Ativa;
        query = query.Where(link => link.Situacao == status);

        int total = await query.CountAsync(cancellationToken);
        List<VinculoProdutoFornecedor> links = await query
            .Include(link => link.Fornecedor)
            .OrderBy(link => link.Fornecedor.Nome)
            .ThenBy(link => link.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedSupplierLinksDto(
            links.Select(link => new SupplierLinkDto(
                link.FornecedorId,
                link.Fornecedor.Nome,
                link.Fornecedor.Documento,
                StatusToString(link.Fornecedor.Situacao),
                StatusToString(link.Situacao),
                link.CriadoEm,
                link.AtualizadoEm,
                link.CriadoPor,
                link.AtualizadoPor)).ToArray(),
            page,
            pageSize,
            total);
    }

    public async Task AssociateAsync(
        Guid productId,
        Guid supplierId,
        Guid actor,
        CancellationToken cancellationToken)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        Produto? product = await dbContext.Produtos
            .SingleOrDefaultAsync(item => item.Id == productId, cancellationToken);
        Fornecedor? supplier = await dbContext.Fornecedores
            .SingleOrDefaultAsync(item => item.Id == supplierId, cancellationToken);
        if (product is null || supplier is null)
        {
            throw new KeyNotFoundException("Produto ou fornecedor não encontrado.");
        }

        if (product.Situacao != SituacaoCadastro.Ativa || supplier.Situacao != SituacaoCadastro.Ativa)
        {
            throw new ProductSupplierConflictException(
                "Somente produtos e fornecedores ativos podem ser associados.");
        }

        VinculoProdutoFornecedor? existing = await dbContext.VinculosProdutoFornecedor
            .SingleOrDefaultAsync(link => link.ProdutoId == productId
                && link.FornecedorId == supplierId, cancellationToken);
        if (existing is null)
        {
            dbContext.VinculosProdutoFornecedor.Add(
                VinculoProdutoFornecedor.Criar(productId, supplierId, actor, DateTimeOffset.UtcNow));
        }
        else if (existing.Situacao == SituacaoCadastro.Inativa)
        {
            existing.DefinirSituacao(SituacaoCadastro.Ativa, actor, DateTimeOffset.UtcNow);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        })
        {
            throw new ProductSupplierConflictException("O produto já está associado a esse fornecedor.");
        }
    }

    public async Task DisassociateAsync(
        Guid productId,
        Guid supplierId,
        Guid actor,
        CancellationToken cancellationToken)
    {
        VinculoProdutoFornecedor? link = await dbContext.VinculosProdutoFornecedor
            .SingleOrDefaultAsync(item =>
                item.ProdutoId == productId
                && item.FornecedorId == supplierId
                && item.Situacao == SituacaoCadastro.Ativa,
                cancellationToken);
        if (link is null)
        {
            throw new KeyNotFoundException("Vínculo ativo entre produto e fornecedor não encontrado.");
        }

        link.DefinirSituacao(SituacaoCadastro.Inativa, actor, DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string StatusToString(SituacaoCadastro status) =>
        status == SituacaoCadastro.Ativa ? "Active" : "Inactive";
}
