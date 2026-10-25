using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Application.Products;

public sealed record CreateProductRequest(string Name, int Quantity, decimal? Price, Guid? ShelfId);

public sealed record UpdateProductRequest(string? Name, int? Quantity, decimal? Price, Guid? ShelfId);

public sealed record SetProductStatusRequest(string Status);

public sealed record ProductDto(
    Guid Id,
    string Name,
    int Quantity,
    decimal? Price,
    Guid? ShelfId,
    string? RackCode,
    string? ShelfCode,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid CreatedBy,
    Guid UpdatedBy);

public sealed record PaginatedProductsDto(
    IReadOnlyList<ProductDto> Items,
    int Page,
    int PageSize,
    int Total);

public sealed class ProductConflictException(string message) : Exception(message)
{
}

public sealed class CatalogForbiddenException(string message) : Exception(message)
{
}

public interface IProductManagementService
{
    Task<PaginatedProductsDto> ListAsync(
        string? search,
        Guid? rackId,
        Guid? shelfId,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<PaginatedProductsDto?> ListAtShelfAsync(
        Guid rackId,
        Guid shelfId,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ProductDto?> FindAsync(Guid productId, CancellationToken cancellationToken);

    Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        Guid actor,
        bool isAdministrator,
        CancellationToken cancellationToken);

    Task<ProductDto?> UpdateAsync(
        Guid productId,
        UpdateProductRequest request,
        Guid actor,
        bool isAdministrator,
        CancellationToken cancellationToken);

    Task SetStatusAsync(
        Guid productId,
        SituacaoCadastro status,
        Guid actor,
        CancellationToken cancellationToken);
}
