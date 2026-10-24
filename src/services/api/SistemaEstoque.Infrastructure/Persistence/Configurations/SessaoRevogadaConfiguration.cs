using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Infrastructure.Persistence.Configurations;

public sealed class SessaoRevogadaConfiguration : IEntityTypeConfiguration<SessaoRevogada>
{
    public void Configure(EntityTypeBuilder<SessaoRevogada> builder)
    {
        builder.ToTable("sessoes_revogadas");
        builder.HasKey(session => session.Identificador);
        builder.Property(session => session.Identificador).HasColumnName("provider_session_id");
        builder.Property(session => session.RevogadaEm).HasColumnName("revoked_at").IsRequired();
    }
}
