using Microsoft.EntityFrameworkCore;
using Npgsql;
using SistemaEstoque.Application.Locations;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Infrastructure.Persistence;

public sealed class LocationManagementService(SistemaEstoqueDbContext dbContext) : ILocationManagementService
{
    public async Task<IReadOnlyList<RackDto>> ListRacksAsync(
        string? status,
        CancellationToken cancellationToken)
    {
        IQueryable<Estante> query = dbContext.Estantes.AsNoTracking().Include(rack => rack.Prateleiras);
        if (TryParseStatus(status, out SituacaoCadastro parsedStatus))
        {
            query = query.Where(rack => rack.Situacao == parsedStatus);
        }

        List<Estante> racks = await query
            .OrderBy(rack => rack.CodigoNormalizado)
            .ToListAsync(cancellationToken);
        return racks.Select(ToDto).ToArray();
    }

    public async Task<RackDto?> FindRackAsync(Guid rackId, CancellationToken cancellationToken)
    {
        Estante? rack = await dbContext.Estantes
            .AsNoTracking()
            .Include(item => item.Prateleiras)
            .SingleOrDefaultAsync(item => item.Id == rackId, cancellationToken);
        return rack is null ? null : ToDto(rack);
    }

    public async Task<RackDto> CreateRackAsync(
        CreateRackRequest request,
        Guid actor,
        CancellationToken cancellationToken)
    {
        Estante rack = Estante.Criar(request.Code, request.Description, actor, DateTimeOffset.UtcNow);
        if (await dbContext.Estantes.AnyAsync(
                item => item.CodigoNormalizado == rack.CodigoNormalizado,
                cancellationToken))
        {
            throw new LocationConflictException("Já existe uma estante com esse código.");
        }

        dbContext.Estantes.Add(rack);
        await SaveChangesAsync(cancellationToken);
        return ToDto(rack);
    }

    public async Task<RackDto?> UpdateRackAsync(
        Guid rackId,
        UpdateRackRequest request,
        Guid actor,
        CancellationToken cancellationToken)
    {
        Estante? rack = await dbContext.Estantes
            .Include(item => item.Prateleiras)
            .SingleOrDefaultAsync(item => item.Id == rackId, cancellationToken);
        if (rack is null)
        {
            return null;
        }

        rack.Atualizar(request.Code, request.Description, actor, DateTimeOffset.UtcNow);
        if (await dbContext.Estantes.AnyAsync(
                item => item.Id != rackId && item.CodigoNormalizado == rack.CodigoNormalizado,
                cancellationToken))
        {
            throw new LocationConflictException("Já existe uma estante com esse código.");
        }

        await SaveChangesAsync(cancellationToken);
        return ToDto(rack);
    }

    public async Task SetRackStatusAsync(
        Guid rackId,
        SituacaoCadastro status,
        Guid actor,
        CancellationToken cancellationToken)
    {
        Estante? rack = await dbContext.Estantes
            .SingleOrDefaultAsync(item => item.Id == rackId, cancellationToken);
        if (rack is null)
        {
            throw new KeyNotFoundException("Estante não encontrada.");
        }

        if (status == SituacaoCadastro.Inativa
            && await dbContext.Prateleiras.AnyAsync(
                shelf => shelf.EstanteId == rackId
                    && dbContext.Produtos.Any(product =>
                        product.PrateleiraId == shelf.Id
                        && product.Situacao == SituacaoCadastro.Ativa),
                cancellationToken))
        {
            throw new LocationConflictException(
                "Não é possível inativar a estante enquanto houver produtos ativos em suas prateleiras.");
        }

        rack.DefinirSituacao(status, actor, DateTimeOffset.UtcNow);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ShelfDto>?> ListShelvesAsync(
        Guid rackId,
        string? status,
        CancellationToken cancellationToken)
    {
        bool rackExists = await dbContext.Estantes.AnyAsync(rack => rack.Id == rackId, cancellationToken);
        if (!rackExists)
        {
            return null;
        }

        IQueryable<Prateleira> query = dbContext.Prateleiras
            .AsNoTracking()
            .Where(shelf => shelf.EstanteId == rackId);
        if (TryParseStatus(status, out SituacaoCadastro parsedStatus))
        {
            query = query.Where(shelf => shelf.Situacao == parsedStatus);
        }

        List<Prateleira> shelves = await query
            .OrderBy(shelf => shelf.CodigoNormalizado)
            .ToListAsync(cancellationToken);
        return shelves.Select(ToDto).ToArray();
    }

    public async Task<ShelfDto> CreateShelfAsync(
        Guid rackId,
        CreateShelfRequest request,
        Guid actor,
        CancellationToken cancellationToken)
    {
        Estante? rack = await dbContext.Estantes
            .SingleOrDefaultAsync(item => item.Id == rackId, cancellationToken);
        if (rack is null)
        {
            throw new KeyNotFoundException("Estante não encontrada.");
        }

        if (rack.Situacao != SituacaoCadastro.Ativa)
        {
            throw new LocationConflictException("Não é possível adicionar prateleiras a uma estante inativa.");
        }

        Prateleira shelf = Prateleira.Criar(rackId, request.Code, actor, DateTimeOffset.UtcNow);
        if (await dbContext.Prateleiras.AnyAsync(
                item => item.EstanteId == rackId && item.CodigoNormalizado == shelf.CodigoNormalizado,
                cancellationToken))
        {
            throw new LocationConflictException("Já existe uma prateleira com esse código nesta estante.");
        }

        dbContext.Prateleiras.Add(shelf);
        await SaveChangesAsync(cancellationToken);
        return ToDto(shelf);
    }

    public async Task<ShelfDto?> UpdateShelfAsync(
        Guid rackId,
        Guid shelfId,
        UpdateShelfRequest request,
        Guid actor,
        CancellationToken cancellationToken)
    {
        Prateleira? shelf = await dbContext.Prateleiras
            .SingleOrDefaultAsync(item => item.EstanteId == rackId && item.Id == shelfId, cancellationToken);
        if (shelf is null)
        {
            return null;
        }

        shelf.Atualizar(request.Code, actor, DateTimeOffset.UtcNow);
        if (await dbContext.Prateleiras.AnyAsync(
                item => item.Id != shelfId
                    && item.EstanteId == rackId
                    && item.CodigoNormalizado == shelf.CodigoNormalizado,
                cancellationToken))
        {
            throw new LocationConflictException("Já existe uma prateleira com esse código nesta estante.");
        }

        await SaveChangesAsync(cancellationToken);
        return ToDto(shelf);
    }

    public async Task SetShelfStatusAsync(
        Guid rackId,
        Guid shelfId,
        SituacaoCadastro status,
        Guid actor,
        CancellationToken cancellationToken)
    {
        Prateleira? shelf = await dbContext.Prateleiras
            .Include(item => item.Estante)
            .SingleOrDefaultAsync(item => item.EstanteId == rackId && item.Id == shelfId, cancellationToken);
        if (shelf is null)
        {
            throw new KeyNotFoundException("Prateleira não encontrada nesta estante.");
        }

        if (status == SituacaoCadastro.Inativa
            && await dbContext.Produtos.AnyAsync(
                product => product.PrateleiraId == shelfId
                    && product.Situacao == SituacaoCadastro.Ativa,
                cancellationToken))
        {
            throw new LocationConflictException(
                "Não é possível inativar a prateleira enquanto houver produtos ativos nela.");
        }

        if (status == SituacaoCadastro.Ativa && shelf.Estante?.Situacao != SituacaoCadastro.Ativa)
        {
            throw new LocationConflictException("Não é possível ativar uma prateleira de estante inativa.");
        }

        shelf.DefinirSituacao(status, actor, DateTimeOffset.UtcNow);
        await SaveChangesAsync(cancellationToken);
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new LocationConflictException("Já existe uma localização com esse código.");
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

    private static RackDto ToDto(Estante rack) =>
        new(
            rack.Id,
            rack.Codigo,
            rack.Descricao,
            ToStatus(rack.Situacao),
            rack.Prateleiras.OrderBy(shelf => shelf.CodigoNormalizado).Select(ToDto).ToArray());

    private static ShelfDto ToDto(Prateleira shelf) =>
        new(shelf.Id, shelf.EstanteId, shelf.Codigo, ToStatus(shelf.Situacao));

    private static string ToStatus(SituacaoCadastro status) =>
        status switch
        {
            SituacaoCadastro.Ativa => "Active",
            SituacaoCadastro.Inativa => "Inactive",
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
}
