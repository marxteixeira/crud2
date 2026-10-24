namespace SistemaEstoque.Application.Authentication;

public sealed class RevokeCurrentSession(ICurrentAccountAuthorization authorization)
{
    public Task ExecuteAsync(Guid sessionId, CancellationToken cancellationToken) =>
        authorization.RevokeSessionAsync(sessionId, cancellationToken);
}
