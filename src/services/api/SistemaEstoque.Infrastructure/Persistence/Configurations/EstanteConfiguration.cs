using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Infrastructure.Persistence.Configurations;

public sealed class EstanteConfiguration : IEntityTypeConfiguration<Estante>
{
    public void Configure(EntityTypeBuilder<Estante> builder)
    {
        builder.ToTable("estantes");
        builder.HasKey(estante => estante.Id);
        builder.Property(estante => estante.Id).HasColumnName("id");
        builder.Property(estante => estante.Codigo).HasColumnName("code").IsRequired();
        builder.Property(estante => estante.CodigoNormalizado).HasColumnName("normalized_code").IsRequired();
        builder.Property(estante => estante.Descricao).HasColumnName("description");
        builder.Property(estante => estante.Situacao)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(estante => estante.CriadoEm).HasColumnName("created_at").IsRequired();
        builder.Property(estante => estante.AtualizadoEm).HasColumnName("updated_at").IsRequired();
        builder.Property(estante => estante.CriadoPor).HasColumnName("created_by").IsRequired();
        builder.Property(estante => estante.AtualizadoPor).HasColumnName("updated_by").IsRequired();
        builder.HasIndex(estante => estante.CodigoNormalizado).IsUnique();
        builder.HasIndex(estante => estante.Situacao);
        builder.HasMany(estante => estante.Prateleiras)
            .WithOne(prateleira => prateleira.Estante)
            .HasForeignKey(prateleira => prateleira.EstanteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
