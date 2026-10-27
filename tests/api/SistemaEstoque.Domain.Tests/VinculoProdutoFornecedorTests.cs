using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Domain.Tests;

public sealed class VinculoProdutoFornecedorTests
{
    [Fact]
    public void Criar_RegistraRelacionamentoEAuditoriaUtc()
    {
        Guid produtoId = Guid.NewGuid();
        Guid fornecedorId = Guid.NewGuid();
        Guid ator = Guid.NewGuid();
        DateTimeOffset agora = DateTimeOffset.Parse("2026-10-26T10:00:00-03:00");

        VinculoProdutoFornecedor vinculo = VinculoProdutoFornecedor.Criar(
            produtoId, fornecedorId, ator, agora);

        Assert.Equal(produtoId, vinculo.ProdutoId);
        Assert.Equal(fornecedorId, vinculo.FornecedorId);
        Assert.Equal(SituacaoCadastro.Ativa, vinculo.Situacao);
        Assert.Equal(ator, vinculo.CriadoPor);
        Assert.Equal(TimeSpan.Zero, vinculo.CriadoEm.Offset);
    }

    [Fact]
    public void DesassociarEReativar_PreservamCriacaoEAtualizamAuditoria()
    {
        Guid atorCriacao = Guid.NewGuid();
        VinculoProdutoFornecedor vinculo = VinculoProdutoFornecedor.Criar(
            Guid.NewGuid(), Guid.NewGuid(), atorCriacao, DateTimeOffset.UtcNow);
        DateTimeOffset alteradoEm = DateTimeOffset.Parse("2026-10-26T12:00:00-03:00");
        Guid atorAtualizacao = Guid.NewGuid();

        vinculo.DefinirSituacao(SituacaoCadastro.Inativa, atorAtualizacao, alteradoEm);
        Assert.Equal(SituacaoCadastro.Inativa, vinculo.Situacao);
        Assert.Equal(atorCriacao, vinculo.CriadoPor);
        Assert.Equal(atorAtualizacao, vinculo.AtualizadoPor);
        Assert.Equal(TimeSpan.Zero, vinculo.AtualizadoEm.Offset);

        vinculo.DefinirSituacao(SituacaoCadastro.Ativa, atorCriacao, alteradoEm.AddMinutes(1));
        Assert.Equal(SituacaoCadastro.Ativa, vinculo.Situacao);
        Assert.Equal(atorCriacao, vinculo.CriadoPor);
        Assert.Equal(atorCriacao, vinculo.AtualizadoPor);
    }

    [Fact]
    public void Criar_RejeitaIdentificadoresOuAtorVazios()
    {
        Assert.Throws<ArgumentException>(() =>
            VinculoProdutoFornecedor.Criar(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() =>
            VinculoProdutoFornecedor.Criar(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() =>
            VinculoProdutoFornecedor.Criar(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, DateTimeOffset.UtcNow));
    }
}
