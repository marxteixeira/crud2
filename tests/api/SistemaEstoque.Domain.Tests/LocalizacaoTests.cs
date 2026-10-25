using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Domain.Tests;

public sealed class LocalizacaoTests
{
    [Fact]
    public void CriarEstante_NormalizaCodigoEGravaAuditoriaUtc()
    {
        Guid ator = Guid.NewGuid();
        DateTimeOffset agora = DateTimeOffset.Parse("2026-10-25T12:00:00-03:00");

        Estante estante = Estante.Criar("  Á ", "  Estoque  ", ator, agora);

        Assert.Equal("Á", estante.Codigo);
        Assert.Equal("a", estante.CodigoNormalizado);
        Assert.Equal("Estoque", estante.Descricao);
        Assert.Equal(SituacaoCadastro.Ativa, estante.Situacao);
        Assert.Equal(TimeSpan.Zero, estante.CriadoEm.Offset);
        Assert.Equal(ator, estante.CriadoPor);
    }

    [Fact]
    public void AtualizarEstante_AtualizaChaveEAuditoria()
    {
        Estante estante = Estante.Criar("A", null, Guid.NewGuid(), DateTimeOffset.UtcNow);
        Guid ator = Guid.NewGuid();
        DateTimeOffset agora = DateTimeOffset.Parse("2026-10-25T10:00:00-03:00");

        estante.Atualizar("B", "  Nova  ", ator, agora);

        Assert.Equal("b", estante.CodigoNormalizado);
        Assert.Equal("Nova", estante.Descricao);
        Assert.Equal(ator, estante.AtualizadoPor);
        Assert.Equal(TimeSpan.Zero, estante.AtualizadoEm.Offset);
    }

    [Fact]
    public void CriarPrateleira_RequerEstante()
    {
        Assert.Throws<ArgumentException>(() =>
            Prateleira.Criar(Guid.Empty, "1", Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CriarPrateleira_NormalizaCodigoERegistraEstante()
    {
        Guid estanteId = Guid.NewGuid();
        Prateleira prateleira = Prateleira.Criar(estanteId, "  Prateleira 1 ", Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(estanteId, prateleira.EstanteId);
        Assert.Equal("prateleira 1", prateleira.CodigoNormalizado);
    }

    [Fact]
    public void DefinirSituacao_RejeitaSituacaoDesconhecida()
    {
        Estante estante = Estante.Criar("A", null, Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            estante.DefinirSituacao((SituacaoCadastro)99, Guid.NewGuid(), DateTimeOffset.UtcNow));
    }
}
