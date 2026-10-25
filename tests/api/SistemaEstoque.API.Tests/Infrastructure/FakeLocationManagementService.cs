using SistemaEstoque.Application.Locations;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.API.Tests.Infrastructure;

internal sealed class FakeLocationManagementService : ILocationManagementService
{
    public IReadOnlyList<RackDto> Racks { get; set; } = [];

    public Guid? LastActor { get; private set; }

    public Task<IReadOnlyList<RackDto>> ListRacksAsync(string? status, CancellationToken cancellationToken) =>
        Task.FromResult(Racks);

    public Task<RackDto?> FindRackAsync(Guid rackId, CancellationToken cancellationToken) =>
        Task.FromResult(Racks.SingleOrDefault(rack => rack.Id == rackId));

    public Task<RackDto> CreateRackAsync(
        CreateRackRequest request,
        Guid actor,
        CancellationToken cancellationToken)
    {
        LastActor = actor;
        RackDto created = new(Guid.NewGuid(), request.Code, request.Description, "Active", []);
        Racks = [.. Racks, created];
        return Task.FromResult(created);
    }

    public Task<RackDto?> UpdateRackAsync(
        Guid rackId,
        UpdateRackRequest request,
        Guid actor,
        CancellationToken cancellationToken) =>
        Task.FromResult<RackDto?>(null);

    public Task SetRackStatusAsync(
        Guid rackId,
        SituacaoCadastro status,
        Guid actor,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<IReadOnlyList<ShelfDto>?> ListShelvesAsync(
        Guid rackId,
        string? status,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ShelfDto>?>([]);

    public Task<ShelfDto> CreateShelfAsync(
        Guid rackId,
        CreateShelfRequest request,
        Guid actor,
        CancellationToken cancellationToken) =>
        Task.FromResult(new ShelfDto(Guid.NewGuid(), rackId, request.Code, "Active"));

    public Task<ShelfDto?> UpdateShelfAsync(
        Guid rackId,
        Guid shelfId,
        UpdateShelfRequest request,
        Guid actor,
        CancellationToken cancellationToken) =>
        Task.FromResult<ShelfDto?>(null);

    public Task SetShelfStatusAsync(
        Guid rackId,
        Guid shelfId,
        SituacaoCadastro status,
        Guid actor,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
