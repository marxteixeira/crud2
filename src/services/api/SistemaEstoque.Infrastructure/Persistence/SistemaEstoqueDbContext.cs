using Microsoft.EntityFrameworkCore;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Infrastructure.Persistence;

public sealed class SistemaEstoqueDbContext : DbContext
{
    public SistemaEstoqueDbContext(DbContextOptions<SistemaEstoqueDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContaDeUsuario> ContasDeUsuario => Set<ContaDeUsuario>();

    public DbSet<SessaoRevogada> SessoesRevogadas => Set<SessaoRevogada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SistemaEstoqueDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
