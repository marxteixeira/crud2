using SistemaEstoque.Application.Authentication;

namespace SistemaEstoque.Application.Users;

public sealed class ConfirmEmailChange(
    IIdentityAdministration identityAdministration,
    IUserManagementService userManagement)
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

        await userManagement.ConfirmEmailAsync(identity, confirmedEmail, cancellationToken);
        return true;
    }
}
