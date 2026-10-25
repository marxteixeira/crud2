using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEstoque.Application.Locations;
using SistemaEstoque.Application.Products;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.API.Controllers;

[ApiController]
[Route("api/locations/racks")]
[Authorize(Policy = "ActiveAccount")]
public sealed class LocationsController(
    ILocationManagementService locations,
    IProductManagementService products,
    IValidator<CreateRackRequest> createRackValidator,
    IValidator<UpdateRackRequest> updateRackValidator,
    IValidator<CreateShelfRequest> createShelfValidator,
    IValidator<UpdateShelfRequest> updateShelfValidator,
    IValidator<SetLocationStatusRequest> statusValidator,
    ILogger<LocationsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RackDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RackDto>>> ListRacks(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        if (!IsValidStatusFilter(status))
        {
            return BadRequest(ValidationError("status", "Situação deve ser Active ou Inactive."));
        }

        IReadOnlyList<RackDto> result = await locations.ListRacksAsync(status, cancellationToken);
        logger.LogInformation(
            "Storage racks listed; ResultCount {ResultCount}; TraceId {TraceId}",
            result.Count,
            HttpContext.TraceIdentifier);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<RackDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<RackDto>> CreateRack(
        [FromBody] CreateRackRequest request,
        CancellationToken cancellationToken)
    {
        ActionResult? validation = await ValidateAsync(request, createRackValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            RackDto created = await locations.CreateRackAsync(request, GetActor(), cancellationToken);
            LogMutation("Storage rack created", created.Id);
            return CreatedAtAction(nameof(GetRack), new { rackId = created.Id }, created);
        }
        catch (LocationConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
    }

    [HttpGet("{rackId:guid}")]
    [ProducesResponseType<RackDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RackDto>> GetRack(Guid rackId, CancellationToken cancellationToken)
    {
        RackDto? rack = await locations.FindRackAsync(rackId, cancellationToken);
        return rack is null ? NotFound() : Ok(rack);
    }

    [HttpPatch("{rackId:guid}")]
    [ProducesResponseType<RackDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RackDto>> UpdateRack(
        Guid rackId,
        [FromBody] UpdateRackRequest request,
        CancellationToken cancellationToken)
    {
        ActionResult? validation = await ValidateAsync(request, updateRackValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            RackDto? updated = await locations.UpdateRackAsync(rackId, request, GetActor(), cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            LogMutation("Storage rack updated", rackId);
            return Ok(updated);
        }
        catch (LocationConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
    }

    [HttpPut("{rackId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetRackStatus(
        Guid rackId,
        [FromBody] SetLocationStatusRequest request,
        CancellationToken cancellationToken)
    {
        ActionResult? validation = await ValidateAsync(request, statusValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            await locations.SetRackStatusAsync(rackId, ParseStatus(request.Status), GetActor(), cancellationToken);
            LogMutation("Storage rack status updated", rackId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (LocationConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
    }

    [HttpGet("{rackId:guid}/shelves")]
    [ProducesResponseType<IReadOnlyList<ShelfDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ShelfDto>>> ListShelves(
        Guid rackId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        if (!IsValidStatusFilter(status))
        {
            return BadRequest(ValidationError("status", "Situação deve ser Active ou Inactive."));
        }

        IReadOnlyList<ShelfDto>? result = await locations.ListShelvesAsync(rackId, status, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{rackId:guid}/shelves")]
    [ProducesResponseType<ShelfDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ShelfDto>> CreateShelf(
        Guid rackId,
        [FromBody] CreateShelfRequest request,
        CancellationToken cancellationToken)
    {
        ActionResult? validation = await ValidateAsync(request, createShelfValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            ShelfDto created = await locations.CreateShelfAsync(rackId, request, GetActor(), cancellationToken);
            LogMutation("Storage shelf created", created.Id);
            return CreatedAtAction(
                nameof(ListShelves),
                new { rackId, status = (string?)null },
                created);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (LocationConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
    }

    [HttpPatch("{rackId:guid}/shelves/{shelfId:guid}")]
    [ProducesResponseType<ShelfDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ShelfDto>> UpdateShelf(
        Guid rackId,
        Guid shelfId,
        [FromBody] UpdateShelfRequest request,
        CancellationToken cancellationToken)
    {
        ActionResult? validation = await ValidateAsync(request, updateShelfValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            ShelfDto? updated = await locations.UpdateShelfAsync(
                rackId, shelfId, request, GetActor(), cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            LogMutation("Storage shelf updated", shelfId);
            return Ok(updated);
        }
        catch (LocationConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
    }

    [HttpPut("{rackId:guid}/shelves/{shelfId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetShelfStatus(
        Guid rackId,
        Guid shelfId,
        [FromBody] SetLocationStatusRequest request,
        CancellationToken cancellationToken)
    {
        ActionResult? validation = await ValidateAsync(request, statusValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            await locations.SetShelfStatusAsync(
                rackId, shelfId, ParseStatus(request.Status), GetActor(), cancellationToken);
            LogMutation("Storage shelf status updated", shelfId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (LocationConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
    }

    [HttpGet("{rackId:guid}/shelves/{shelfId:guid}/products")]
    [ProducesResponseType<PaginatedProductsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedProductsDto>> ListProductsAtShelf(
        Guid rackId,
        Guid shelfId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100 || !IsValidStatusFilter(status))
        {
            return BadRequest(ValidationError("query", "Os filtros de paginação ou situação são inválidos."));
        }

        PaginatedProductsDto? result = await products.ListAtShelfAsync(
            rackId, shelfId, status, page, pageSize, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    private async Task<ActionResult?> ValidateAsync<TRequest>(
        TRequest request,
        IValidator<TRequest> validator,
        CancellationToken cancellationToken)
    {
        FluentValidation.Results.ValidationResult result =
            await validator.ValidateAsync(request, cancellationToken);
        if (result.IsValid)
        {
            return null;
        }

        Dictionary<string, string[]> errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
        return BadRequest(new ValidationProblemDetails(errors));
    }

    private Guid GetActor() =>
        Guid.TryParse(User.FindFirstValue("account_id"), out Guid actor)
            ? actor
            : throw new UnauthorizedAccessException("The authenticated account identifier is invalid.");

    private void LogMutation(string eventName, Guid recordId) =>
        logger.LogInformation(
            "{EventName}; RecordId {RecordId}; TraceId {TraceId}",
            eventName,
            recordId,
            HttpContext.TraceIdentifier);

    private ConflictObjectResult ConflictProblem(string detail) =>
        Conflict(Problem(
            statusCode: StatusCodes.Status409Conflict,
            detail: detail));

    private static ValidationProblemDetails ValidationError(string key, string message) =>
        new(new Dictionary<string, string[]> { [key] = [message] });

    private static bool IsValidStatusFilter(string? status) =>
        status is null or "Active" or "Inactive";

    private static SituacaoCadastro ParseStatus(string status) =>
        status switch
        {
            "Active" => SituacaoCadastro.Ativa,
            "Inactive" => SituacaoCadastro.Inativa,
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
}
