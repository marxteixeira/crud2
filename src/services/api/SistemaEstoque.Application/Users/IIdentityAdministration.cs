namespace SistemaEstoque.Application.Users;

public interface IIdentityAdministration
{
    Task<Guid> InviteAsync(string email, string displayName, CancellationToken cancellationToken);

    Task DeleteAsync(Guid identity, CancellationToken cancellationToken);

    Task UpdateRoleAsync(Guid identity, string role, CancellationToken cancellationToken);

    Task RequestEmailChangeAsync(Guid identity, string email, CancellationToken cancellationToken);

    Task<string?> GetConfirmedEmailAsync(string accessToken, CancellationToken cancellationToken);
}
