using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using SistemaEstoque.Application.Users;
using SistemaEstoque.Domain.Entities;
using SistemaEstoque.Infrastructure.Identity;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.API.Tests;

public sealed class UserManagementPerformanceTests
{
    private const int AccountCount = 1000;
    private const int RequestCount = 100;
    private static readonly TimeSpan MaxP95 = TimeSpan.FromSeconds(3);

    [Fact]
    public async Task List_With1000Accounts_CompletesWithinP95Limit()
    {
        DbContextOptions<SistemaEstoqueDbContext> options = new DbContextOptionsBuilder<SistemaEstoqueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        await using SistemaEstoqueDbContext context = new(options);
        for (int index = 0; index < AccountCount; index++)
        {
            context.ContasDeUsuario.Add(ContaDeUsuario.CriarConvite(
                Guid.NewGuid(),
                $"user{index}@example.com",
                $"User {index:D4}",
                index % 2 == 0 ? PerfilUsuario.Operador : PerfilUsuario.Administrador,
                "bootstrap",
                DateTimeOffset.UtcNow));
        }

        await context.SaveChangesAsync();
        UserManagementService service = new(
            context,
            new UnusedIdentityAdministration(),
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator(),
            NullLogger<UserManagementService>.Instance);
        await service.ListAsync(null, null, 1, 20, CancellationToken.None);

        List<TimeSpan> durations = [];
        for (int request = 0; request < RequestCount; request++)
        {
            string? search = request % 3 == 0 ? $"user{request}" : null;
            int page = search is null ? 1 + request % 5 : 1;
            long start = Stopwatch.GetTimestamp();
            PaginatedUsersDto result = await service.ListAsync(search, null, page, 20, CancellationToken.None);
            durations.Add(Stopwatch.GetElapsedTime(start));
            Assert.NotEmpty(result.Items);
        }

        TimeSpan p95 = durations.OrderBy(duration => duration).ElementAt((int)Math.Ceiling(RequestCount * 0.95) - 1);

        Assert.True(p95 <= MaxP95, $"p95 de {p95.TotalMilliseconds:F1} ms excede {MaxP95.TotalMilliseconds:F0} ms.");
    }

    private sealed class UnusedIdentityAdministration : IIdentityAdministration
    {
        public Task<Guid> InviteAsync(string email, string displayName, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task DeleteAsync(Guid identity, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task UpdateRoleAsync(Guid identity, string role, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task RequestEmailChangeAsync(Guid identity, string email, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<string?> GetConfirmedEmailAsync(string accessToken, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
