using SistemaEstoque.Application.Users;

namespace SistemaEstoque.Application.Authentication;

public sealed class ActivateInvitedAccount(
    ICurrentAccountAuthorization authorization,
    IIdentityAdministration identityAdministration)
{
    public async Task<bool> ExecuteAsync(
        Guid identity,
        string accessToken,
        CancellationToken cancellationToken)
    {
        string? confirmedEmail = await identityAdministration.GetConfirmedEmailAsync(
            accessToken,
            cancellationToken);
        if (confirmedEmail is null)
        {
            return false;
        }

        await authorization.ActivatePendingAccountAsync(
            identity,
            confirmedEmail,
            identity.ToString(),
            cancellationToken);
        return true;
    }
}
