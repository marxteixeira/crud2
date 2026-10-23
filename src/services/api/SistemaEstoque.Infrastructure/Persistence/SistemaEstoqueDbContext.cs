using Microsoft.EntityFrameworkCore;

namespace SistemaEstoque.Infrastructure.Persistence;

public sealed class SistemaEstoqueDbContext : DbContext
{
    public SistemaEstoqueDbContext(DbContextOptions<SistemaEstoqueDbContext> options)
        : base(options)
    {
    }
}
