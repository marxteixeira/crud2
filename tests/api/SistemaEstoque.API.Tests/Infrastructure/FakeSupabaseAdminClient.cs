using SistemaEstoque.Application.Users;

namespace SistemaEstoque.API.Tests.Infrastructure;

internal sealed class FakeSupabaseAdminClient : IIdentityAdministration
{
    public string? ConfirmedEmail { get; set; } = "user@example.com";

    public Task<Guid> InviteAsync(string email, string displayName, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task DeleteAsync(Guid identity, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task UpdateRoleAsync(Guid identity, string role, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task RequestEmailChangeAsync(Guid identity, string email, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<string?> GetConfirmedEmailAsync(string accessToken, CancellationToken cancellationToken) =>
        Task.FromResult(ConfirmedEmail);
}
