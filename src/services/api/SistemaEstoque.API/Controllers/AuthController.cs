using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEstoque.Application.Authentication;
using SistemaEstoque.Application.Users;

namespace SistemaEstoque.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    GetCurrentAccount getCurrentAccount,
    ActivateInvitedAccount activateInvitedAccount,
    RevokeCurrentSession revokeCurrentSession,
    ConfirmEmailChange confirmEmailChange,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Policy = "ActiveAccount")]
    [ProducesResponseType<CurrentAccount>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentAccount>> GetCurrent(CancellationToken cancellationToken)
    {
        Guid identity = GetSubject();
        CurrentAccount? account = await getCurrentAccount.ExecuteAsync(identity, cancellationToken);
        if (account is null || account.Status != "Ativa")
        {
            return Unauthorized();
        }

        logger.LogInformation(
            "Authenticated account profile retrieved; AccountId {AccountId}; TraceId {TraceId}",
            account.Id,
            HttpContext.TraceIdentifier);
        return Ok(account);
    }

    [HttpPost("activation")]
    [Authorize(Policy = "InvitedAccount")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(CancellationToken cancellationToken)
    {
        string? accessToken = Request.Headers.Authorization
            .ToString()
            .Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Unauthorized();
        }

        Guid subject = GetSubject();
        if (!await activateInvitedAccount.ExecuteAsync(subject, accessToken, cancellationToken))
        {
            return Conflict(Problem(
                statusCode: StatusCodes.Status409Conflict,
                detail: "A confirmação do e-mail ainda não foi concluída."));
        }

        logger.LogInformation(
            "Invited account activated; AccountId {AccountId}; TraceId {TraceId}",
            User.FindFirstValue("account_id"),
            HttpContext.TraceIdentifier);
        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("session_id"), out Guid sessionId))
        {
            return Unauthorized();
        }

        await revokeCurrentSession.ExecuteAsync(sessionId, cancellationToken);
        logger.LogInformation(
            "Authentication session revoked; TraceId {TraceId}",
            HttpContext.TraceIdentifier);
        return NoContent();
    }

    [HttpPost("email-change-confirmation")]
    [Authorize(Policy = "ActiveAccount")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmEmailChange(CancellationToken cancellationToken)
    {
        string? accessToken = Request.Headers.Authorization
            .ToString()
            .Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Unauthorized();
        }

        Guid subject = GetSubject();
        if (!await confirmEmailChange.ExecuteAsync(subject, accessToken, cancellationToken))
        {
            return Conflict(Problem(
                statusCode: StatusCodes.Status409Conflict,
                detail: "Não há troca de e-mail confirmada para esta identidade."));
        }

        logger.LogInformation(
            "User email change confirmed; AccountId {AccountId}; TraceId {TraceId}",
            User.FindFirstValue("account_id"),
            HttpContext.TraceIdentifier);
        return NoContent();
    }

    private Guid GetSubject() =>
        Guid.TryParse(User.FindFirstValue("sub"), out Guid subject)
            ? subject
            : throw new UnauthorizedAccessException("The authenticated subject is invalid.");
}
