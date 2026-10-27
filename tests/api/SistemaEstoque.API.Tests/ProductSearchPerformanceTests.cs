using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SistemaEstoque.Application.Products;
using SistemaEstoque.Domain.Entities;
using SistemaEstoque.Infrastructure.Persistence;

namespace SistemaEstoque.API.Tests;

public sealed class ProductSearchPerformanceTests
{
    private const int ProductCount = 5000;
    private const int RequestCount = 100;
    private static readonly TimeSpan MaxP95 = TimeSpan.FromSeconds(2);

    [Fact]
    public async Task Search_With5000Products_CompletesWithinP95Limit()
    {
        DbContextOptions<SistemaEstoqueDbContext> options = new DbContextOptionsBuilder<SistemaEstoqueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        await using SistemaEstoqueDbContext context = new(options);
        Guid actor = Guid.NewGuid();
        for (int index = 0; index < ProductCount; index++)
        {
            context.Produtos.Add(Produto.Criar(
                $"Produto {index:D4}",
                index,
                null,
                null,
                SituacaoCadastro.Inativa,
                actor,
                DateTimeOffset.UtcNow));
        }

        await context.SaveChangesAsync();
        ProductManagementService service = new(context);
        await service.ListAsync("produto", null, null, null, null, 1, 25, CancellationToken.None);

        List<TimeSpan> durations = [];
        for (int request = 0; request < RequestCount; request++)
        {
            long start = Stopwatch.GetTimestamp();
            PaginatedProductsDto result = await service.ListAsync(
                "produto", null, null, null, null, 1, 25, CancellationToken.None);
            durations.Add(Stopwatch.GetElapsedTime(start));
            Assert.NotEmpty(result.Items);
        }

        TimeSpan p95 = durations.OrderBy(duration => duration)
            .ElementAt((int)Math.Ceiling(RequestCount * 0.95) - 1);
        Assert.True(p95 <= MaxP95, $"p95 de {p95.TotalMilliseconds:F1} ms excede {MaxP95.TotalMilliseconds:F0} ms.");
    }
}
