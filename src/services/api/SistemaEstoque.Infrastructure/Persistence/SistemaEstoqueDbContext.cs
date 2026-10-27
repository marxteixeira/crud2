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

    public DbSet<Estante> Estantes => Set<Estante>();

    public DbSet<Prateleira> Prateleiras => Set<Prateleira>();

    public DbSet<Produto> Produtos => Set<Produto>();

    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();

    public DbSet<VinculoProdutoFornecedor> VinculosProdutoFornecedor => Set<VinculoProdutoFornecedor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SistemaEstoqueDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
