using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Application.Users;

public sealed record CreateUserRequest(string Email, string DisplayName, string Role);

public sealed record UpdateUserRequest(string? Email, string? DisplayName, string? Role);

public sealed record UserAccountDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? PendingEmail,
    string Role,
    string Status);

public sealed record PaginatedUsersDto(
    IReadOnlyList<UserAccountDto> Items,
    int Page,
    int PageSize,
    int Total);

public sealed class AccountConflictException(string message) : Exception(message)
{
}

public interface IUserManagementService
{
    Task<PaginatedUsersDto> ListAsync(
        string? search,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<UserAccountDto?> FindAsync(Guid id, CancellationToken cancellationToken);

    Task<UserAccountDto> InviteAsync(
        CreateUserRequest request,
        string actor,
        CancellationToken cancellationToken);

    Task<UserAccountDto> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        string actor,
        CancellationToken cancellationToken);

    Task SetStatusAsync(
        Guid id,
        SituacaoConta status,
        string actor,
        CancellationToken cancellationToken);

    Task ResendInvitationAsync(Guid id, CancellationToken cancellationToken);

    Task ConfirmEmailAsync(
        Guid identity,
        string confirmedEmail,
        CancellationToken cancellationToken);
}
