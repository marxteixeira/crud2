using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Infrastructure.Persistence.Configurations;

public sealed class VinculoProdutoFornecedorConfiguration : IEntityTypeConfiguration<VinculoProdutoFornecedor>
{
    public void Configure(EntityTypeBuilder<VinculoProdutoFornecedor> builder)
    {
        builder.ToTable("produtos_fornecedores");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).HasColumnName("id");
        builder.Property(link => link.ProdutoId).HasColumnName("product_id");
        builder.Property(link => link.FornecedorId).HasColumnName("supplier_id");
        builder.Property(link => link.Situacao)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(link => link.CriadoEm).HasColumnName("created_at").IsRequired();
        builder.Property(link => link.AtualizadoEm).HasColumnName("updated_at").IsRequired();
        builder.Property(link => link.CriadoPor).HasColumnName("created_by").IsRequired();
        builder.Property(link => link.AtualizadoPor).HasColumnName("updated_by").IsRequired();
        builder.HasIndex(link => new { link.ProdutoId, link.FornecedorId })
            .IsUnique()
            .HasFilter("status = 'Ativa'");
        builder.HasIndex(link => new { link.FornecedorId, link.Situacao });
        builder.HasIndex(link => new { link.ProdutoId, link.Situacao });
        builder.HasOne(link => link.Produto)
            .WithMany(product => product.Fornecedores)
            .HasForeignKey(link => link.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(link => link.Fornecedor)
            .WithMany(supplier => supplier.Vinculos)
            .HasForeignKey(link => link.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
