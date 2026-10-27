using SistemaEstoque.Application.Suppliers;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.API.Tests.Infrastructure;

internal sealed class FakeSupplierManagementService : ISupplierManagementService
{
    public PaginatedSuppliersDto ListResult { get; set; } = new([], 1, 25, 0);

    public SupplierDto? Supplier { get; set; }

    public Guid? LastActor { get; private set; }

    public CreateSupplierRequest? LastCreateRequest { get; private set; }

    public UpdateSupplierRequest? LastUpdateRequest { get; private set; }

    public SituacaoCadastro? LastStatus { get; private set; }

    public (string? Search, string? Status, int Page, int PageSize)? LastFilters { get; private set; }

    public Exception? CreateFailure { get; set; }

    public Exception? UpdateFailure { get; set; }

    public Exception? StatusFailure { get; set; }

    public PaginatedProductSupplierLinksDto? ProductLinks { get; set; } = new([], 1, 25, 0);

    public (Guid SupplierId, string? ProductStatus, string? LinkStatus, int Page, int PageSize)? LastProductFilters { get; private set; }

    public Task<PaginatedSuppliersDto> ListAsync(
        string? search, string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        LastFilters = (search, status, page, pageSize);
        return Task.FromResult(ListResult with { Page = page, PageSize = pageSize });
    }

    public Task<SupplierDto?> FindAsync(Guid supplierId, CancellationToken cancellationToken) =>
        Task.FromResult(Supplier?.Id == supplierId ? Supplier : null);

    public Task<SupplierDto> CreateAsync(
        CreateSupplierRequest request, Guid actor, CancellationToken cancellationToken)
    {
        if (CreateFailure is not null)
        {
            throw CreateFailure;
        }

        LastActor = actor;
        LastCreateRequest = request;
        return Task.FromResult(Supplier ?? new SupplierDto(
            Guid.NewGuid(), request.Name, request.TaxIdentifier, request.Phone, request.Email,
            request.ContactName, request.Notes, "Active", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, actor, actor));
    }

    public Task<SupplierDto?> UpdateAsync(
        Guid supplierId, UpdateSupplierRequest request, Guid actor, CancellationToken cancellationToken)
    {
        if (UpdateFailure is not null)
        {
            throw UpdateFailure;
        }

        LastActor = actor;
        LastUpdateRequest = request;
        return Task.FromResult(Supplier?.Id == supplierId ? Supplier : null);
    }

    public Task SetStatusAsync(
        Guid supplierId, SituacaoCadastro status, Guid actor, CancellationToken cancellationToken)
    {
        if (StatusFailure is not null)
        {
            throw StatusFailure;
        }

        LastActor = actor;
        LastStatus = status;
        return Task.CompletedTask;
    }

    public Task<PaginatedProductSupplierLinksDto?> ListProductsAsync(
        Guid supplierId, string? productStatus, string? linkStatus, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        LastProductFilters = (supplierId, productStatus, linkStatus, page, pageSize);
        return Task.FromResult(ProductLinks is null ? null : ProductLinks with { Page = page, PageSize = pageSize });
    }
}
