using Microsoft.EntityFrameworkCore;
using Npgsql;
using SistemaEstoque.Application.Suppliers;
using SistemaEstoque.Domain.Entities;
using SistemaEstoque.Domain.ValueObjects;
using System.Data;

namespace SistemaEstoque.Infrastructure.Persistence;

public sealed class SupplierManagementService(SistemaEstoqueDbContext dbContext) : ISupplierManagementService
{
    public async Task<PaginatedSuppliersDto> ListAsync(
        string? search,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Fornecedor> query = dbContext.Fornecedores.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            string termo = search.Trim();
            query = query.Where(supplier =>
                EF.Functions.ILike(supplier.Nome, $"%{EscapeLike(termo)}%")
                || supplier.Documento.Contains(termo));
        }

        if (TryParseStatus(status, out SituacaoCadastro situacao))
        {
            query = query.Where(supplier => supplier.Situacao == situacao);
        }

        int total = await query.CountAsync(cancellationToken);
        List<Fornecedor> suppliers = await query
            .OrderBy(supplier => supplier.Nome)
            .ThenBy(supplier => supplier.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedSuppliersDto(suppliers.Select(ToDto).ToArray(), page, pageSize, total);
    }

    public async Task<SupplierDto?> FindAsync(Guid supplierId, CancellationToken cancellationToken)
    {
        Fornecedor? supplier = await dbContext.Fornecedores
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == supplierId, cancellationToken);
        return supplier is null ? null : ToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(
        CreateSupplierRequest request,
        Guid actor,
        CancellationToken cancellationToken)
    {
        Fornecedor supplier = Fornecedor.Criar(
            request.Name,
            request.TaxIdentifier,
            request.Phone,
            request.Email,
            request.ContactName,
            request.Notes,
            actor,
            DateTimeOffset.UtcNow);
        if (await dbContext.Fornecedores.AnyAsync(
                item => item.Documento == supplier.Documento,
                cancellationToken))
        {
            throw new SupplierConflictException("Já existe um fornecedor com esse CPF ou CNPJ.");
        }

        dbContext.Fornecedores.Add(supplier);
        await SaveChangesAsync(cancellationToken);
        return ToDto(supplier);
    }

    public async Task<SupplierDto?> UpdateAsync(
        Guid supplierId,
        UpdateSupplierRequest request,
        Guid actor,
        CancellationToken cancellationToken)
    {
        Fornecedor? supplier = await dbContext.Fornecedores
            .SingleOrDefaultAsync(item => item.Id == supplierId, cancellationToken);
        if (supplier is null)
        {
            return null;
        }

        string documento = CpfCnpj.Criar(request.TaxIdentifier).Digitos;
        if (await dbContext.Fornecedores.AnyAsync(
                item => item.Id != supplierId && item.Documento == documento,
                cancellationToken))
        {
            throw new SupplierConflictException("Já existe um fornecedor com esse CPF ou CNPJ.");
        }

        supplier.Atualizar(
            request.Name,
            request.TaxIdentifier,
            request.Phone,
            request.Email,
            request.ContactName,
            request.Notes,
            actor,
            DateTimeOffset.UtcNow);
        await SaveChangesAsync(cancellationToken);
        return ToDto(supplier);
    }

    public async Task SetStatusAsync(
        Guid supplierId,
        SituacaoCadastro status,
        Guid actor,
        CancellationToken cancellationToken)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        Fornecedor? supplier = await dbContext.Fornecedores
            .SingleOrDefaultAsync(item => item.Id == supplierId, cancellationToken);
        if (supplier is null)
        {
            throw new KeyNotFoundException("Fornecedor não encontrado.");
        }

        if (status == SituacaoCadastro.Inativa
            && await dbContext.VinculosProdutoFornecedor.AnyAsync(
                link => link.FornecedorId == supplierId
                    && link.Situacao == SituacaoCadastro.Ativa
                    && link.Produto.Situacao == SituacaoCadastro.Ativa,
                cancellationToken))
        {
            throw new SupplierConflictException(
                "Não é possível inativar o fornecedor enquanto houver produtos ativos vinculados.");
        }

        supplier.DefinirSituacao(status, actor, DateTimeOffset.UtcNow);
        await SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<PaginatedProductSupplierLinksDto?> ListProductsAsync(
        Guid supplierId,
        string? productStatus,
        string? linkStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        bool supplierExists = await dbContext.Fornecedores.AsNoTracking()
            .AnyAsync(item => item.Id == supplierId, cancellationToken);
        if (!supplierExists)
        {
            return null;
        }

        IQueryable<VinculoProdutoFornecedor> query = dbContext.VinculosProdutoFornecedor
            .AsNoTracking()
            .Where(link => link.FornecedorId == supplierId);
        if (TryParseStatus(linkStatus, out SituacaoCadastro parsedLinkStatus))
        {
            query = query.Where(link => link.Situacao == parsedLinkStatus);
        }
        else
        {
            query = query.Where(link => link.Situacao == SituacaoCadastro.Ativa);
        }

        if (TryParseStatus(productStatus, out SituacaoCadastro parsedProductStatus))
        {
            query = query.Where(link => link.Produto.Situacao == parsedProductStatus);
        }

        int total = await query.CountAsync(cancellationToken);
        List<VinculoProdutoFornecedor> links = await query
            .Include(link => link.Produto)
            .OrderBy(link => link.Produto.NomeNormalizado)
            .ThenBy(link => link.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedProductSupplierLinksDto(
            links.Select(link => new ProductSupplierLinkDto(
                link.ProdutoId,
                link.Produto.Nome,
                StatusToString(link.Produto.Situacao),
                StatusToString(link.Situacao),
                link.CriadoEm,
                link.AtualizadoEm,
                link.CriadoPor,
                link.AtualizadoPor)).ToArray(),
            page,
            pageSize,
            total);
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        })
        {
            throw new SupplierConflictException("Já existe um fornecedor com esse CPF ou CNPJ.");
        }
    }

    private static SupplierDto ToDto(Fornecedor supplier) =>
        new(
            supplier.Id,
            supplier.Nome,
            supplier.Documento,
            supplier.Telefone,
            supplier.Email,
            supplier.NomeContato,
            supplier.Observacoes,
            StatusToString(supplier.Situacao),
            supplier.CriadoEm,
            supplier.AtualizadoEm,
            supplier.CriadoPor,
            supplier.AtualizadoPor);

    private static bool TryParseStatus(string? status, out SituacaoCadastro parsedStatus)
    {
        parsedStatus = status switch
        {
            "Active" => SituacaoCadastro.Ativa,
            "Inactive" => SituacaoCadastro.Inativa,
            _ => default
        };
        return status is "Active" or "Inactive";
    }

    private static string StatusToString(SituacaoCadastro status) =>
        status == SituacaoCadastro.Ativa ? "Active" : "Inactive";

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
