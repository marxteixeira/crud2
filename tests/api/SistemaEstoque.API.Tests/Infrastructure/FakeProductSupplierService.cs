using SistemaEstoque.Application.Suppliers;

namespace SistemaEstoque.API.Tests.Infrastructure;

internal sealed class FakeProductSupplierService : IProductSupplierService
{
    public PaginatedSupplierLinksDto? SupplierLinks { get; set; } = new([], 1, 25, 0);

    public Exception? AssociateFailure { get; set; }

    public Exception? DisassociateFailure { get; set; }

    public Guid? LastActor { get; private set; }

    public (Guid ProductId, Guid SupplierId)? LastPair { get; private set; }

    public (Guid ProductId, string? LinkStatus, int Page, int PageSize)? LastFilters { get; private set; }

    public Task<PaginatedSupplierLinksDto?> ListSuppliersAsync(
        Guid productId, string? linkStatus, int page, int pageSize, CancellationToken cancellationToken)
    {
        LastFilters = (productId, linkStatus, page, pageSize);
        return Task.FromResult(SupplierLinks is null ? null : SupplierLinks with { Page = page, PageSize = pageSize });
    }

    public Task AssociateAsync(Guid productId, Guid supplierId, Guid actor, CancellationToken cancellationToken)
    {
        LastPair = (productId, supplierId);
        LastActor = actor;
        if (AssociateFailure is not null)
        {
            throw AssociateFailure;
        }

        return Task.CompletedTask;
    }

    public Task DisassociateAsync(Guid productId, Guid supplierId, Guid actor, CancellationToken cancellationToken)
    {
        LastPair = (productId, supplierId);
        LastActor = actor;
        if (DisassociateFailure is not null)
        {
            throw DisassociateFailure;
        }

        return Task.CompletedTask;
    }
}
