using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Infrastructure.Persistence.Configurations;

public sealed class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("fornecedores");
        builder.HasKey(supplier => supplier.Id);
        builder.Property(supplier => supplier.Id).HasColumnName("id");
        builder.Property(supplier => supplier.Nome).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(supplier => supplier.Documento)
            .HasColumnName("tax_identifier")
            .HasMaxLength(14)
            .IsRequired();
        builder.Property(supplier => supplier.Telefone).HasColumnName("phone").HasMaxLength(30);
        builder.Property(supplier => supplier.Email).HasColumnName("email").HasMaxLength(254);
        builder.Property(supplier => supplier.NomeContato).HasColumnName("contact_name").HasMaxLength(200);
        builder.Property(supplier => supplier.Observacoes).HasColumnName("notes").HasMaxLength(2000);
        builder.Property(supplier => supplier.Situacao)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(supplier => supplier.CriadoEm).HasColumnName("created_at").IsRequired();
        builder.Property(supplier => supplier.AtualizadoEm).HasColumnName("updated_at").IsRequired();
        builder.Property(supplier => supplier.CriadoPor).HasColumnName("created_by").IsRequired();
        builder.Property(supplier => supplier.AtualizadoPor).HasColumnName("updated_by").IsRequired();
        builder.HasIndex(supplier => supplier.Documento).IsUnique();
        builder.HasIndex(supplier => supplier.Situacao);
    }
}
