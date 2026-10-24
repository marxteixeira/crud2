using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Domain.Tests;

public sealed class ContaDeUsuarioTests
{
    [Fact]
    public void CriarConvite_NormalizaEmailEIniciaPendente()
    {
        DateTimeOffset agora = DateTimeOffset.Parse("2026-10-23T12:00:00-03:00");

        ContaDeUsuario conta = ContaDeUsuario.CriarConvite(
            Guid.NewGuid(), "  USER@Example.com ", " Nome ", PerfilUsuario.Operador, "bootstrap", agora);

        Assert.Equal("user@example.com", conta.Email);
        Assert.Equal("Nome", conta.NomeExibicao);
        Assert.Equal(SituacaoConta.ConvitePendente, conta.Situacao);
        Assert.Equal(TimeSpan.Zero, conta.CriadoEm.Offset);
    }

    [Fact]
    public void CriarConvite_RejeitaNomeVazio()
    {
        Assert.Throws<ArgumentException>(() => ContaDeUsuario.CriarConvite(
            Guid.NewGuid(), "user@example.com", " ", PerfilUsuario.Operador, "bootstrap", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CriarConvite_RejeitaPerfilDesconhecido()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ContaDeUsuario.CriarConvite(
            Guid.NewGuid(), "user@example.com", "Usuário", (PerfilUsuario)99, "bootstrap", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Atualizar_RejeitaPerfilDesconhecido()
    {
        ContaDeUsuario conta = ContaDeUsuario.CriarConvite(
            Guid.NewGuid(), "user@example.com", "Usuário", PerfilUsuario.Operador, "bootstrap", DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            conta.Atualizar(null, null, (PerfilUsuario)99, "admin", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Ativar_RecusaContaInativa()
    {
        ContaDeUsuario conta = ContaDeUsuario.CriarConvite(
            Guid.NewGuid(), "user@example.com", "Usuário", PerfilUsuario.Operador, "bootstrap", DateTimeOffset.UtcNow);
        conta.DefinirSituacao(SituacaoConta.Inativa, DateTimeOffset.UtcNow, "admin");

        Assert.Throws<InvalidOperationException>(() => conta.Ativar(DateTimeOffset.UtcNow, "user"));
    }

    [Fact]
    public void Ativar_AtivaConviteERegistraAuditoriaEmUtc()
    {
        ContaDeUsuario conta = ContaDeUsuario.CriarConvite(
            Guid.NewGuid(), "user@example.com", "Usuário", PerfilUsuario.Operador, "bootstrap", DateTimeOffset.UtcNow);
        DateTimeOffset alteracao = DateTimeOffset.Parse("2026-10-23T12:00:00-03:00");

        conta.Ativar(alteracao, "titular");

        Assert.Equal(SituacaoConta.Ativa, conta.Situacao);
        Assert.Equal(TimeSpan.Zero, conta.AtualizadoEm.Offset);
        Assert.Equal("titular", conta.AtualizadoPor);
    }

    [Fact]
    public void Atualizar_PreservaIdentidadeEAdiaNovoEmailAteConfirmacao()
    {
        Guid identity = Guid.NewGuid();
        ContaDeUsuario conta = ContaDeUsuario.CriarConvite(
            identity, "current@example.com", "Usuário", PerfilUsuario.Operador, "bootstrap", DateTimeOffset.UtcNow);

        conta.Atualizar("new@example.com", "Novo nome", PerfilUsuario.Administrador, "admin", DateTimeOffset.UtcNow);

        Assert.Equal(identity, conta.Identidade);
        Assert.Equal("current@example.com", conta.Email);
        Assert.Equal("new@example.com", conta.EmailPendente);
        Assert.Equal("Novo nome", conta.NomeExibicao);
        Assert.Equal(PerfilUsuario.Administrador, conta.Perfil);
    }
}
