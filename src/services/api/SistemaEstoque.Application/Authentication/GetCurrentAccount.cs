namespace SistemaEstoque.Application.Authentication;

public sealed class GetCurrentAccount(ICurrentAccountAuthorization authorization)
{
    public Task<CurrentAccount?> ExecuteAsync(Guid identity, CancellationToken cancellationToken) =>
        authorization.FindByIdentityAsync(identity, cancellationToken);
}
