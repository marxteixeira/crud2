using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Application.Locations;

public sealed record CreateRackRequest(string Code, string? Description);

public sealed record UpdateRackRequest(string Code, string? Description);

public sealed record CreateShelfRequest(string Code);

public sealed record UpdateShelfRequest(string Code);

public sealed record SetLocationStatusRequest(string Status);

public sealed record ShelfDto(Guid Id, Guid RackId, string Code, string Status);

public sealed record RackDto(
    Guid Id,
    string Code,
    string? Description,
    string Status,
    IReadOnlyList<ShelfDto> Shelves);

public sealed class LocationConflictException(string message) : Exception(message)
{
}

public interface ILocationManagementService
{
    Task<IReadOnlyList<RackDto>> ListRacksAsync(string? status, CancellationToken cancellationToken);

    Task<RackDto?> FindRackAsync(Guid rackId, CancellationToken cancellationToken);

    Task<RackDto> CreateRackAsync(
        CreateRackRequest request,
        Guid actor,
        CancellationToken cancellationToken);

    Task<RackDto?> UpdateRackAsync(
        Guid rackId,
        UpdateRackRequest request,
        Guid actor,
        CancellationToken cancellationToken);

    Task SetRackStatusAsync(
        Guid rackId,
        SituacaoCadastro status,
        Guid actor,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ShelfDto>?> ListShelvesAsync(
        Guid rackId,
        string? status,
        CancellationToken cancellationToken);

    Task<ShelfDto> CreateShelfAsync(
        Guid rackId,
        CreateShelfRequest request,
        Guid actor,
        CancellationToken cancellationToken);

    Task<ShelfDto?> UpdateShelfAsync(
        Guid rackId,
        Guid shelfId,
        UpdateShelfRequest request,
        Guid actor,
        CancellationToken cancellationToken);

    Task SetShelfStatusAsync(
        Guid rackId,
        Guid shelfId,
        SituacaoCadastro status,
        Guid actor,
        CancellationToken cancellationToken);
}
