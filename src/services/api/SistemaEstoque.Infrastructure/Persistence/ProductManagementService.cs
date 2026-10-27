using Microsoft.EntityFrameworkCore;
using Npgsql;
using SistemaEstoque.Application.Products;
using SistemaEstoque.Domain.Entities;
using SistemaEstoque.Domain.ValueObjects;

namespace SistemaEstoque.Infrastructure.Persistence;

public sealed class ProductManagementService(SistemaEstoqueDbContext dbContext) : IProductManagementService
{
    public async Task<PaginatedProductsDto> ListAsync(
        string? search,
        Guid? rackId,
        Guid? shelfId,
        string? status,
        Guid? supplierId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Produto> query = dbContext.Produtos
            .AsNoTracking()
            .Include(product => product.Prateleira)
            .ThenInclude(shelf => shelf!.Estante);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string normalizedSearch = ChaveNormalizada.Criar(search);
            query = query.Where(product => product.NomeNormalizado.Contains(normalizedSearch));
        }

        if (rackId.HasValue)
        {
            query = query.Where(product =>
                product.Prateleira != null && product.Prateleira.EstanteId == rackId.Value);
        }

        if (shelfId.HasValue)
        {
            query = query.Where(product => product.PrateleiraId == shelfId.Value);
        }

        if (TryParseStatus(status, out SituacaoCadastro parsedStatus))
        {
            query = query.Where(product => product.Situacao == parsedStatus);
        }

        if (supplierId.HasValue)
        {
            query = query.Where(product => product.Fornecedores.Any(link =>
                link.FornecedorId == supplierId.Value && link.Situacao == SituacaoCadastro.Ativa));
        }

        int total = await query.CountAsync(cancellationToken);
        List<Produto> products = await query
            .OrderBy(product => product.NomeNormalizado)
            .ThenBy(product => product.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PaginatedProductsDto(products.Select(ToDto).ToArray(), page, pageSize, total);
    }

    public async Task<PaginatedProductsDto?> ListAtShelfAsync(
        Guid rackId,
        Guid shelfId,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        bool shelfExists = await dbContext.Prateleiras.AsNoTracking()
            .AnyAsync(shelf => shelf.Id == shelfId && shelf.EstanteId == rackId, cancellationToken);
        if (!shelfExists)
        {
            return null;
        }

        return await ListAsync(null, rackId, shelfId, status, null, page, pageSize, cancellationToken);
    }

    public async Task<ProductDto?> FindAsync(Guid productId, CancellationToken cancellationToken)
    {
        Produto? product = await dbContext.Produtos
            .AsNoTracking()
            .Include(item => item.Prateleira)
            .ThenInclude(shelf => shelf!.Estante)
            .SingleOrDefaultAsync(item => item.Id == productId, cancellationToken);
        return product is null ? null : ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        Guid actor,
        bool isAdministrator,
        CancellationToken cancellationToken)
    {
        if (!isAdministrator && request.Price.HasValue)
        {
            throw new CatalogForbiddenException("Somente Administradores podem definir preços.");
        }

        bool hasValidLocation = request.ShelfId.HasValue
            && await IsActiveLocationAsync(request.ShelfId.Value, cancellationToken);
        if (request.ShelfId.HasValue && !hasValidLocation)
        {
            throw new ProductConflictException("A prateleira informada não está disponível.");
        }

        SituacaoCadastro status = isAdministrator && request.Price.HasValue && hasValidLocation
            ? SituacaoCadastro.Ativa
            : SituacaoCadastro.Inativa;
        Produto product = Produto.Criar(
            request.Name,
            request.Quantity,
            request.Price,
            request.ShelfId,
            status,
            actor,
            DateTimeOffset.UtcNow);

        if (await dbContext.Produtos.AnyAsync(
                item => item.NomeNormalizado == product.NomeNormalizado,
                cancellationToken))
        {
            throw new ProductConflictException("Já existe um produto com esse nome.");
        }

        dbContext.Produtos.Add(product);
        await SaveChangesAsync(cancellationToken);
        return await FindAsync(product.Id, cancellationToken)
            ?? throw new InvalidOperationException("O produto criado não foi encontrado.");
    }

    public async Task<ProductDto?> UpdateAsync(
        Guid productId,
        UpdateProductRequest request,
        Guid actor,
        bool isAdministrator,
        CancellationToken cancellationToken)
    {
        if (!isAdministrator && request.Price.HasValue)
        {
            throw new CatalogForbiddenException("Somente Administradores podem definir ou alterar preços.");
        }

        Produto? product = await dbContext.Produtos
            .Include(item => item.Prateleira)
            .ThenInclude(shelf => shelf!.Estante)
            .SingleOrDefaultAsync(item => item.Id == productId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        Guid? candidateShelfId = request.ShelfId ?? product.PrateleiraId;
        bool locationValid = candidateShelfId.HasValue
            && await IsActiveLocationAsync(candidateShelfId.Value, cancellationToken);
        if (request.ShelfId.HasValue && !locationValid)
        {
            throw new ProductConflictException("A prateleira informada não está disponível.");
        }

        string? normalizedName = request.Name is null ? null : ChaveNormalizada.Criar(request.Name);
        if (normalizedName is not null
            && await dbContext.Produtos.AnyAsync(
                item => item.Id != productId && item.NomeNormalizado == normalizedName,
                cancellationToken))
        {
            throw new ProductConflictException("Já existe um produto com esse nome.");
        }

        product.Atualizar(
            request.Name,
            request.Quantity,
            request.Price,
            request.ShelfId,
            locationValid,
            actor,
            DateTimeOffset.UtcNow);
        await SaveChangesAsync(cancellationToken);
        return await FindAsync(productId, cancellationToken);
    }

    public async Task SetStatusAsync(
        Guid productId,
        SituacaoCadastro status,
        Guid actor,
        CancellationToken cancellationToken)
    {
        Produto? product = await dbContext.Produtos
            .SingleOrDefaultAsync(item => item.Id == productId, cancellationToken);
        if (product is null)
        {
            throw new KeyNotFoundException("Produto não encontrado.");
        }

        bool locationValid = product.PrateleiraId.HasValue
            && await IsActiveLocationAsync(product.PrateleiraId.Value, cancellationToken);
        product.DefinirSituacao(status, locationValid, actor, DateTimeOffset.UtcNow);
        await SaveChangesAsync(cancellationToken);
    }

    private Task<bool> IsActiveLocationAsync(Guid shelfId, CancellationToken cancellationToken) =>
        dbContext.Prateleiras.AsNoTracking()
            .AnyAsync(
                shelf => shelf.Id == shelfId
                    && shelf.Situacao == SituacaoCadastro.Ativa
                    && shelf.Estante != null
                    && shelf.Estante.Situacao == SituacaoCadastro.Ativa,
                cancellationToken);

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ProductConflictException("Já existe um produto com esse nome.");
        }
    }

    private static bool TryParseStatus(string? value, out SituacaoCadastro status)
    {
        status = value switch
        {
            "Active" => SituacaoCadastro.Ativa,
            "Inactive" => SituacaoCadastro.Inativa,
            _ => default
        };
        return value is "Active" or "Inactive";
    }

    private static ProductDto ToDto(Produto product) =>
        new(
            product.Id,
            product.Nome,
            product.Quantidade,
            product.Preco,
            product.PrateleiraId,
            product.Prateleira?.Estante?.Codigo,
            product.Prateleira?.Codigo,
            product.Situacao == SituacaoCadastro.Ativa ? "Active" : "Inactive",
            product.CriadoEm,
            product.AtualizadoEm,
            product.CriadoPor,
            product.AtualizadoPor);
}
