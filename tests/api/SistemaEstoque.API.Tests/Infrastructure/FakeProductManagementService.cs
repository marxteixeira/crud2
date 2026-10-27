using SistemaEstoque.Application.Products;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.API.Tests.Infrastructure;

internal sealed class FakeProductManagementService : IProductManagementService
{
    public PaginatedProductsDto ListResult { get; set; } = new([], 1, 25, 0);

    public PaginatedProductsDto? ShelfResult { get; set; } = new([], 1, 25, 0);

    public ProductDto? Product { get; set; }

    public CreateProductRequest? LastCreateRequest { get; private set; }

    public Guid? LastActor { get; private set; }

    public bool? LastIsAdministrator { get; private set; }

    public (string? Search, Guid? RackId, Guid? ShelfId, string? Status, Guid? SupplierId, int Page, int PageSize)? LastFilters { get; private set; }

    public (Guid RackId, Guid ShelfId, string? Status, int Page, int PageSize)? LastShelfFilters { get; private set; }

    public Task<PaginatedProductsDto> ListAsync(
        string? search, Guid? rackId, Guid? shelfId, string? status, Guid? supplierId, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        LastFilters = (search, rackId, shelfId, status, supplierId, page, pageSize);
        return Task.FromResult(ListResult with { Page = page, PageSize = pageSize });
    }

    public Task<PaginatedProductsDto?> ListAtShelfAsync(
        Guid rackId, Guid shelfId, string? status, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        LastShelfFilters = (rackId, shelfId, status, page, pageSize);
        return Task.FromResult(ShelfResult is null ? null : ShelfResult with { Page = page, PageSize = pageSize });
    }

    public Task<ProductDto?> FindAsync(Guid productId, CancellationToken cancellationToken) =>
        Task.FromResult(Product?.Id == productId ? Product : null);

    public Task<ProductDto> CreateAsync(
        CreateProductRequest request, Guid actor, bool isAdministrator, CancellationToken cancellationToken)
    {
        LastCreateRequest = request;
        LastActor = actor;
        LastIsAdministrator = isAdministrator;
        return Task.FromResult(Product ?? new ProductDto(
            Guid.NewGuid(), request.Name, request.Quantity, request.Price, request.ShelfId,
            null, null, "Inactive", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, actor, actor));
    }

    public Task<ProductDto?> UpdateAsync(
        Guid productId, UpdateProductRequest request, Guid actor, bool isAdministrator,
        CancellationToken cancellationToken)
    {
        LastActor = actor;
        LastIsAdministrator = isAdministrator;
        return Task.FromResult(Product?.Id == productId ? Product : null);
    }

    public Task SetStatusAsync(
        Guid productId, SituacaoCadastro status, Guid actor, CancellationToken cancellationToken)
    {
        LastActor = actor;
        return Task.CompletedTask;
    }
}
