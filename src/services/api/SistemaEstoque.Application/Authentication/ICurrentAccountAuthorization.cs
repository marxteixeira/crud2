namespace SistemaEstoque.Application.Authentication;

public sealed record CurrentAccount(
    Guid Id,
    Guid Identity,
    string Email,
    string DisplayName,
    string Role,
    string Status);

public interface ICurrentAccountAuthorization
{
    Task<CurrentAccount?> FindByIdentityAsync(Guid identity, CancellationToken cancellationToken);

    Task<bool> IsSessionRevokedAsync(Guid sessionId, CancellationToken cancellationToken);

    Task ActivatePendingAccountAsync(
        Guid identity,
        string confirmedEmail,
        string actor,
        CancellationToken cancellationToken);

    Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken);
}
