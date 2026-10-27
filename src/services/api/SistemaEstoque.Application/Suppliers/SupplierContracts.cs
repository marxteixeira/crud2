namespace SistemaEstoque.Application.Suppliers;

public sealed record CreateSupplierRequest(
    string Name,
    string TaxIdentifier,
    string? Phone,
    string? Email,
    string? ContactName,
    string? Notes);

public sealed record UpdateSupplierRequest(
    string Name,
    string TaxIdentifier,
    string? Phone,
    string? Email,
    string? ContactName,
    string? Notes);

public sealed record SetSupplierStatusRequest(string Status);

public sealed record SupplierDto(
    Guid Id,
    string Name,
    string TaxIdentifier,
    string? Phone,
    string? Email,
    string? ContactName,
    string? Notes,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid CreatedBy,
    Guid UpdatedBy);

public sealed record PaginatedSuppliersDto(
    IReadOnlyList<SupplierDto> Items,
    int Page,
    int PageSize,
    int Total);

public sealed record SupplierLinkDto(
    Guid SupplierId,
    string SupplierName,
    string TaxIdentifier,
    string SupplierStatus,
    string LinkStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid CreatedBy,
    Guid UpdatedBy);

public sealed record ProductSupplierLinkDto(
    Guid ProductId,
    string ProductName,
    string ProductStatus,
    string LinkStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid CreatedBy,
    Guid UpdatedBy);

public sealed record PaginatedSupplierLinksDto(
    IReadOnlyList<SupplierLinkDto> Items,
    int Page,
    int PageSize,
    int Total);

public sealed record PaginatedProductSupplierLinksDto(
    IReadOnlyList<ProductSupplierLinkDto> Items,
    int Page,
    int PageSize,
    int Total);

public sealed class ProductSupplierConflictException(string message) : Exception(message)
{
}

public sealed class SupplierConflictException(string message) : Exception(message)
{
}

public interface ISupplierManagementService
{
    Task<PaginatedSuppliersDto> ListAsync(
        string? search,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<SupplierDto?> FindAsync(Guid supplierId, CancellationToken cancellationToken);

    Task<SupplierDto> CreateAsync(
        CreateSupplierRequest request,
        Guid actor,
        CancellationToken cancellationToken);

    Task<SupplierDto?> UpdateAsync(
        Guid supplierId,
        UpdateSupplierRequest request,
        Guid actor,
        CancellationToken cancellationToken);

    Task SetStatusAsync(
        Guid supplierId,
        SistemaEstoque.Domain.Entities.SituacaoCadastro status,
        Guid actor,
        CancellationToken cancellationToken);

    Task<PaginatedProductSupplierLinksDto?> ListProductsAsync(
        Guid supplierId,
        string? productStatus,
        string? linkStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

public interface IProductSupplierService
{
    Task<PaginatedSupplierLinksDto?> ListSuppliersAsync(
        Guid productId,
        string? linkStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task AssociateAsync(
        Guid productId,
        Guid supplierId,
        Guid actor,
        CancellationToken cancellationToken);

    Task DisassociateAsync(
        Guid productId,
        Guid supplierId,
        Guid actor,
        CancellationToken cancellationToken);
}
