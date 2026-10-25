using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Infrastructure.Persistence.Configurations;

public sealed class PrateleiraConfiguration : IEntityTypeConfiguration<Prateleira>
{
    public void Configure(EntityTypeBuilder<Prateleira> builder)
    {
        builder.ToTable("prateleiras");
        builder.HasKey(prateleira => prateleira.Id);
        builder.Property(prateleira => prateleira.Id).HasColumnName("id");
        builder.Property(prateleira => prateleira.EstanteId).HasColumnName("rack_id").IsRequired();
        builder.Property(prateleira => prateleira.Codigo).HasColumnName("code").IsRequired();
        builder.Property(prateleira => prateleira.CodigoNormalizado).HasColumnName("normalized_code").IsRequired();
        builder.Property(prateleira => prateleira.Situacao)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(prateleira => prateleira.CriadoEm).HasColumnName("created_at").IsRequired();
        builder.Property(prateleira => prateleira.AtualizadoEm).HasColumnName("updated_at").IsRequired();
        builder.Property(prateleira => prateleira.CriadoPor).HasColumnName("created_by").IsRequired();
        builder.Property(prateleira => prateleira.AtualizadoPor).HasColumnName("updated_by").IsRequired();
        builder.HasIndex(prateleira => new { prateleira.EstanteId, prateleira.CodigoNormalizado }).IsUnique();
        builder.HasIndex(prateleira => prateleira.Situacao);
    }
}
