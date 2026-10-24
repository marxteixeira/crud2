using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEstoque.Application.Users;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "Administrator")]
public sealed class UsersController(
    IUserManagementService userManagement,
    ILogger<UsersController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginatedUsersDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedUsersDto>> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        bool validStatus = status is null
            || (Enum.TryParse(status, false, out SituacaoConta parsedStatus) && Enum.IsDefined(parsedStatus));
        if (page < 1 || pageSize is < 1 or > 100 || !validStatus)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["query"] = ["Os filtros de paginação ou situação são inválidos."]
            }));
        }

        PaginatedUsersDto result = await userManagement.ListAsync(
            search, status, page, pageSize, cancellationToken);
        logger.LogInformation(
            "User accounts listed; Page {Page}; PageSize {PageSize}; ResultCount {ResultCount}; TraceId {TraceId}",
            page,
            pageSize,
            result.Items.Count,
            HttpContext.TraceIdentifier);
        return Ok(result);
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType<UserAccountDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserAccountDto>> Get(Guid userId, CancellationToken cancellationToken)
    {
        UserAccountDto? account = await userManagement.FindAsync(userId, cancellationToken);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    [ProducesResponseType<UserAccountDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserAccountDto>> Invite(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            UserAccountDto created = await userManagement.InviteAsync(
                request,
                User.FindFirstValue("account_id") ?? "unknown",
                cancellationToken);
            logger.LogInformation(
                "User invitation created; AccountId {AccountId}; TraceId {TraceId}",
                created.Id,
                HttpContext.TraceIdentifier);
            return CreatedAtAction(nameof(Get), new { userId = created.Id }, created);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new ValidationProblemDetails(exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray())));
        }
    }

    [HttpPatch("{userId:guid}")]
    [ProducesResponseType<UserAccountDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserAccountDto>> Update(
        Guid userId,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            UserAccountDto updated = await userManagement.UpdateAsync(
                userId,
                request,
                User.FindFirstValue("account_id") ?? "unknown",
                cancellationToken);
            logger.LogInformation(
                "User account updated; AccountId {AccountId}; TraceId {TraceId}",
                updated.Id,
                HttpContext.TraceIdentifier);
            return Ok(updated);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new ValidationProblemDetails(exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray())));
        }
    }

    [HttpPut("{userId:guid}/activation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetActivation(
        Guid userId,
        [FromBody] SetUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(request.Status, false, out SituacaoConta status)
            || !Enum.IsDefined(status)
            || status == SituacaoConta.ConvitePendente)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["status"] = ["Situação deve ser Ativa ou Inativa."]
            }));
        }

        await userManagement.SetStatusAsync(
            userId,
            status,
            User.FindFirstValue("account_id") ?? "unknown",
            cancellationToken);
        logger.LogInformation(
            "User activation status updated; AccountId {AccountId}; Status {Status}; TraceId {TraceId}",
            userId,
            status,
            HttpContext.TraceIdentifier);
        return NoContent();
    }

    public sealed record SetUserStatusRequest(string Status);
}
