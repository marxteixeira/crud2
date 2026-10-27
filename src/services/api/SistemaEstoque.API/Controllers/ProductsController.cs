using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEstoque.Application.Products;
using SistemaEstoque.Application.Suppliers;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Policy = "ActiveAccount")]
public sealed class ProductsController(
    IProductManagementService products,
    IProductSupplierService supplierLinks,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator,
    IValidator<SetProductStatusRequest> statusValidator,
    ILogger<ProductsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginatedProductsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedProductsDto>> List(
        [FromQuery] string? search,
        [FromQuery] Guid? rackId,
        [FromQuery] Guid? shelfId,
        [FromQuery] string? status,
        [FromQuery] Guid? supplierId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidQuery(page, pageSize, status))
        {
            return BadRequest(ValidationError("query", "Os filtros de paginação ou situação são inválidos."));
        }

        PaginatedProductsDto result = await products.ListAsync(
            search, rackId, shelfId, status, supplierId, page, pageSize, cancellationToken);
        logger.LogInformation(
            "Products listed; Page {Page}; PageSize {PageSize}; ResultCount {ResultCount}; TraceId {TraceId}",
            page,
            pageSize,
            result.Items.Count,
            HttpContext.TraceIdentifier);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        ActionResult? validation = await ValidateAsync(request, createValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        if (!IsAdministrator() && request.Price.HasValue)
        {
            return ForbidProblem("Somente Administradores podem definir preços.");
        }

        try
        {
            ProductDto created = await products.CreateAsync(
                request, GetActor(), IsAdministrator(), cancellationToken);
            LogMutation("Product created", created.Id, created.Status);
            return CreatedAtAction(nameof(Get), new { productId = created.Id }, created);
        }
        catch (CatalogForbiddenException exception)
        {
            return ForbidProblem(exception.Message);
        }
        catch (ProductConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(ValidationError("product", exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ValidationError("product", exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return ConflictProblem(exception.Message);
        }
    }

    [HttpGet("{productId:guid}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Get(Guid productId, CancellationToken cancellationToken)
    {
        ProductDto? product = await products.FindAsync(productId, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPatch("{productId:guid}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> Update(
        Guid productId,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        ActionResult? validation = await ValidateAsync(request, updateValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        if (!IsAdministrator() && request.Price.HasValue)
        {
            return ForbidProblem("Somente Administradores podem definir ou alterar preços.");
        }

        try
        {
            ProductDto? updated = await products.UpdateAsync(
                productId, request, GetActor(), IsAdministrator(), cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            LogMutation("Product updated", productId, updated.Status);
            return Ok(updated);
        }
        catch (CatalogForbiddenException exception)
        {
            return ForbidProblem(exception.Message);
        }
        catch (ProductConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(ValidationError("product", exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ValidationError("product", exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return ConflictProblem(exception.Message);
        }
    }

    [HttpPut("{productId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetStatus(
        Guid productId,
        [FromBody] SetProductStatusRequest request,
        CancellationToken cancellationToken)
    {
        ActionResult? validation = await ValidateAsync(request, statusValidator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            SituacaoCadastro status = ParseStatus(request.Status);
            if (status == SituacaoCadastro.Ativa && !IsAdministrator())
            {
                return ForbidProblem("Somente Administradores podem ativar produtos.");
            }

            await products.SetStatusAsync(productId, status, GetActor(), cancellationToken);
            LogMutation("Product status updated", productId, request.Status);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ProductConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return ConflictProblem(exception.Message);
        }
    }

    [HttpGet("{productId:guid}/suppliers")]
    [ProducesResponseType<PaginatedSupplierLinksDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedSupplierLinksDto>> ListSuppliers(
        Guid productId,
        [FromQuery] string? linkStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidStatus(linkStatus) || page < 1 || pageSize is < 1 or > 100)
        {
            return BadRequest(ValidationError("query", "Os filtros de paginação ou situação são inválidos."));
        }

        PaginatedSupplierLinksDto? result = await supplierLinks.ListSuppliersAsync(
            productId, linkStatus, page, pageSize, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{productId:guid}/suppliers/{supplierId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssociateSupplier(
        Guid productId,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        try
        {
            await supplierLinks.AssociateAsync(productId, supplierId, GetActor(), cancellationToken);
            LogMutation("Product supplier associated", productId, supplierId.ToString());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ProductSupplierConflictException exception)
        {
            return ConflictProblem(exception.Message);
        }
    }

    [HttpDelete("{productId:guid}/suppliers/{supplierId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DisassociateSupplier(
        Guid productId,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        try
        {
            await supplierLinks.DisassociateAsync(productId, supplierId, GetActor(), cancellationToken);
            LogMutation("Product supplier disassociated", productId, supplierId.ToString());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
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

    private void LogMutation(string eventName, Guid productId, string status) =>
        logger.LogInformation(
            "{EventName}; ProductId {ProductId}; Status {Status}; TraceId {TraceId}",
            eventName,
            productId,
            status,
            HttpContext.TraceIdentifier);

    private ConflictObjectResult ConflictProblem(string detail) =>
        Conflict(Problem(statusCode: StatusCodes.Status409Conflict, detail: detail));

    private ObjectResult ForbidProblem(string detail) =>
        StatusCode(StatusCodes.Status403Forbidden, Problem(statusCode: StatusCodes.Status403Forbidden, detail: detail));

    private static ValidationProblemDetails ValidationError(string key, string message) =>
        new(new Dictionary<string, string[]> { [key] = [message] });

    private static bool IsValidQuery(int page, int pageSize, string? status) =>
        page >= 1 && pageSize is >= 1 and <= 100 && status is null or "Active" or "Inactive";

    private static bool IsValidStatus(string? status) => status is null or "Active" or "Inactive";

    private static SituacaoCadastro ParseStatus(string status) =>
        status switch
        {
            "Active" => SituacaoCadastro.Ativa,
            "Inactive" => SituacaoCadastro.Inativa,
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
}
