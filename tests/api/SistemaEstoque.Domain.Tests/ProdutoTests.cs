using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Domain.Tests;

public sealed class ProdutoTests
{
    [Fact]
    public void Criar_NormalizaNomeEPermiteProdutoInativoSemPreco()
    {
        Produto produto = Produto.Criar(
            "  Café  ", 2, null, null, SituacaoCadastro.Inativa, Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal("Café", produto.Nome);
        Assert.Equal("cafe", produto.NomeNormalizado);
        Assert.Null(produto.Preco);
        Assert.Equal(SituacaoCadastro.Inativa, produto.Situacao);
    }

    [Fact]
    public void Criar_RejeitaNomeVazioEQuantidadeNegativa()
    {
        Assert.Throws<ArgumentException>(() => Produto.Criar(
            " ", 0, 1m, Guid.NewGuid(), SituacaoCadastro.Inativa, Guid.NewGuid(), DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentOutOfRangeException>(() => Produto.Criar(
            "Produto", -1, null, null, SituacaoCadastro.Inativa, Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.234)]
    public void Criar_RejeitaPrecoInvalido(decimal preco)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Produto.Criar(
            "Produto", 1, preco, null, SituacaoCadastro.Inativa, Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Criar_RejeitaProdutoAtivoSemPrecoOuLocalizacao()
    {
        Assert.Throws<InvalidOperationException>(() => Produto.Criar(
            "Produto", 1, 10m, null, SituacaoCadastro.Ativa, Guid.NewGuid(), DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() => Produto.Criar(
            "Produto", 1, null, Guid.NewGuid(), SituacaoCadastro.Ativa, Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Ativar_ExigePrecoELocalizacaoAtiva()
    {
        Produto produto = Produto.Criar(
            "Produto", 1, 10m, Guid.NewGuid(), SituacaoCadastro.Inativa, Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            produto.DefinirSituacao(SituacaoCadastro.Ativa, false, Guid.NewGuid(), DateTimeOffset.UtcNow));

        produto.DefinirSituacao(SituacaoCadastro.Ativa, true, Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(SituacaoCadastro.Ativa, produto.Situacao);
    }

    [Fact]
    public void Atualizar_AtualizaDadosNomeNormalizadoEAuditoria()
    {
        Produto produto = Produto.Criar(
            "Produto", 1, null, null, SituacaoCadastro.Inativa, Guid.NewGuid(), DateTimeOffset.UtcNow);
        Guid ator = Guid.NewGuid();
        DateTimeOffset agora = DateTimeOffset.Parse("2026-10-25T10:00:00-03:00");

        produto.Atualizar("Pêssego", 4, 0m, null, false, ator, agora);

        Assert.Equal("pessego", produto.NomeNormalizado);
        Assert.Equal(4, produto.Quantidade);
        Assert.Equal(0m, produto.Preco);
        Assert.Equal(ator, produto.AtualizadoPor);
        Assert.Equal(TimeSpan.Zero, produto.AtualizadoEm.Offset);
    }
}
