using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Infrastructure.Persistence.Configurations;

public sealed class ContaDeUsuarioConfiguration : IEntityTypeConfiguration<ContaDeUsuario>
{
    public void Configure(EntityTypeBuilder<ContaDeUsuario> builder)
    {
        builder.ToTable("contas_de_usuario");
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).HasColumnName("id");
        builder.Property(account => account.Identidade).HasColumnName("identity_subject").IsRequired();
        builder.Property(account => account.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(account => account.EmailPendente).HasColumnName("pending_email").HasMaxLength(320);
        builder.Property(account => account.NomeExibicao).HasColumnName("display_name").HasMaxLength(200).IsRequired();
        builder.Property(account => account.Perfil).HasColumnName("role").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(account => account.Situacao).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(account => account.CriadoEm).HasColumnName("created_at").IsRequired();
        builder.Property(account => account.AtualizadoEm).HasColumnName("updated_at").IsRequired();
        builder.Property(account => account.CriadoPor).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(account => account.AtualizadoPor).HasColumnName("updated_by").HasMaxLength(100).IsRequired();
        builder.HasIndex(account => account.Identidade).IsUnique();
        builder.HasIndex(account => account.Email).IsUnique();
        builder.HasIndex(account => account.EmailPendente).IsUnique()
            .HasFilter("\"pending_email\" IS NOT NULL");
    }
}
