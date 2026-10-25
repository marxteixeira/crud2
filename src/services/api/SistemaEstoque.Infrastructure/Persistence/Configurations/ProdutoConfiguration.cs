using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Infrastructure.Persistence.Configurations;

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable(
            "produtos",
            table =>
            {
                table.HasCheckConstraint("ck_produtos_quantity_nonnegative", "quantity >= 0");
                table.HasCheckConstraint("ck_produtos_price_nonnegative", "price IS NULL OR price >= 0");
            });
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).HasColumnName("id");
        builder.Property(product => product.Nome).HasColumnName("name").IsRequired();
        builder.Property(product => product.NomeNormalizado).HasColumnName("normalized_name").IsRequired();
        builder.Property(product => product.Quantidade).HasColumnName("quantity").IsRequired();
        builder.Property(product => product.Preco).HasColumnName("price").HasPrecision(18, 2);
        builder.Property(product => product.PrateleiraId).HasColumnName("shelf_id");
        builder.Property(product => product.Situacao)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(product => product.CriadoEm).HasColumnName("created_at").IsRequired();
        builder.Property(product => product.AtualizadoEm).HasColumnName("updated_at").IsRequired();
        builder.Property(product => product.CriadoPor).HasColumnName("created_by").IsRequired();
        builder.Property(product => product.AtualizadoPor).HasColumnName("updated_by").IsRequired();
        builder.HasIndex(product => product.NomeNormalizado).IsUnique();
        builder.HasIndex(product => product.Situacao);
        builder.HasIndex(product => product.PrateleiraId);
        builder.HasOne(product => product.Prateleira)
            .WithMany()
            .HasForeignKey(product => product.PrateleiraId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
