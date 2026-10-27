using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEstoque.Application.Suppliers;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.API.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize(Policy = "ActiveAccount")]
public sealed class SuppliersController(
    ISupplierManagementService suppliers,
    IValidator<CreateSupplierRequest> createValidator,
    IValidator<UpdateSupplierRequest> updateValidator,
    IValidator<SetSupplierStatusRequest> statusValidator,
    ILogger<SuppliersController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginatedSuppliersDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedSuppliersDto>> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100 || !IsValidStatus(status))
        {
            return BadRequest(ValidationError("query", "Os filtros de paginação ou situação são inválidos."));
        }

        PaginatedSuppliersDto result = await suppliers.ListAsync(
            search, status, page, pageSize, cancellationToken);
        logger.LogInformation(
            "Suppliers listed; Page {Page}; PageSize {PageSize}; ResultCount {ResultCount}; TraceId {TraceId}",
            page,
            pageSize,
            result.Items.Count,
            HttpContext.TraceIdentifier);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<SupplierDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<SupplierDto>> Create(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAdministrator())
        {
            return ForbidProblem("Somente Administradores podem cadastrar fornecedores.");
        }

        ActionResult? validation = await ValidateAsync(request, createValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            SupplierDto created = await suppliers.CreateAsync(request, GetActor(), cancellationToken);
            LogMutation("Supplier created", created.Id, created.Status);
            return CreatedAtAction(nameof(Get), new { supplierId = created.Id }, created);
        }
        catch (SupplierConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ValidationError("supplier", exception.Message));
        }
    }

    [HttpGet("{supplierId:guid}")]
    [ProducesResponseType<SupplierDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierDto>> Get(Guid supplierId, CancellationToken cancellationToken)
    {
        SupplierDto? supplier = await suppliers.FindAsync(supplierId, cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPatch("{supplierId:guid}")]
    [ProducesResponseType<SupplierDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SupplierDto>> Update(
        Guid supplierId,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAdministrator())
        {
            return ForbidProblem("Somente Administradores podem editar fornecedores.");
        }

        ActionResult? validation = await ValidateAsync(request, updateValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            SupplierDto? updated = await suppliers.UpdateAsync(
                supplierId, request, GetActor(), cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            LogMutation("Supplier updated", updated.Id, updated.Status);
            return Ok(updated);
        }
        catch (SupplierConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ValidationError("supplier", exception.Message));
        }
    }

    [HttpPut("{supplierId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetStatus(
        Guid supplierId,
        [FromBody] SetSupplierStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAdministrator())
        {
            return ForbidProblem("Somente Administradores podem alterar a situação do fornecedor.");
        }

        ActionResult? validation = await ValidateAsync(request, statusValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            SituacaoCadastro status = request.Status == "Active"
                ? SituacaoCadastro.Ativa
                : SituacaoCadastro.Inativa;
            await suppliers.SetStatusAsync(supplierId, status, GetActor(), cancellationToken);
            LogMutation("Supplier status updated", supplierId, request.Status);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (SupplierConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
    }

    [HttpGet("{supplierId:guid}/products")]
    [ProducesResponseType<PaginatedProductSupplierLinksDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedProductSupplierLinksDto>> ListProducts(
        Guid supplierId,
        [FromQuery] string? status,
        [FromQuery] string? linkStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidStatus(status)
            || !IsValidStatus(linkStatus)
            || page < 1
            || pageSize is < 1 or > 100)
        {
            return BadRequest(ValidationError("query", "Os filtros de paginação ou situação são inválidos."));
        }

        PaginatedProductSupplierLinksDto? result = await suppliers.ListProductsAsync(
            supplierId, status, linkStatus, page, pageSize, cancellationToken);
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

    private bool IsAdministrator() => User.IsInRole("Administrador");

    private void LogMutation(string eventName, Guid supplierId, string status) =>
        logger.LogInformation(
            "{EventName}; SupplierId {SupplierId}; Status {Status}; TraceId {TraceId}",
            eventName,
            supplierId,
            status,
            HttpContext.TraceIdentifier);

    private ConflictObjectResult ConflictProblem(string detail) =>
        Conflict(Problem(statusCode: StatusCodes.Status409Conflict, detail: detail));

    private ObjectResult ForbidProblem(string detail) =>
        StatusCode(StatusCodes.Status403Forbidden, Problem(statusCode: StatusCodes.Status403Forbidden, detail: detail));

    private static ValidationProblemDetails ValidationError(string key, string message) =>
        new(new Dictionary<string, string[]> { [key] = [message] });

    private static bool IsValidStatus(string? status) =>
        status is null or "Active" or "Inactive";
}
