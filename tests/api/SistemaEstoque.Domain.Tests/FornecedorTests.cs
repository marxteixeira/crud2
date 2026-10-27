using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Domain.Tests;

public sealed class FornecedorTests
{
    [Fact]
    public void Criar_NormalizaNomeDocumentoEOpcionaisERegistraAuditoriaUtc()
    {
        Guid ator = Guid.NewGuid();
        DateTimeOffset agora = DateTimeOffset.Parse("2026-10-26T10:00:00-03:00");

        Fornecedor fornecedor = Fornecedor.Criar(
            "  Fornecedor Teste ",
            "529.982.247-25",
            " 11999999999 ",
            "contato@example.com",
            "  Maria ",
            "  Observação ",
            ator,
            agora);

        Assert.Equal("Fornecedor Teste", fornecedor.Nome);
        Assert.Equal("52998224725", fornecedor.Documento);
        Assert.Equal("11999999999", fornecedor.Telefone);
        Assert.Equal("contato@example.com", fornecedor.Email);
        Assert.Equal("Maria", fornecedor.NomeContato);
        Assert.Equal("Observação", fornecedor.Observacoes);
        Assert.Equal(SituacaoCadastro.Ativa, fornecedor.Situacao);
        Assert.Equal(ator, fornecedor.CriadoPor);
        Assert.Equal(TimeSpan.Zero, fornecedor.CriadoEm.Offset);
    }

    [Theory]
    [InlineData("", "52998224725")]
    [InlineData("Nome", "123")]
    public void Criar_RejeitaNomeOuDocumentoInvalido(string nome, string documento)
    {
        Assert.Throws<ArgumentException>(() => Fornecedor.Criar(
            nome, documento, null, null, null, null, Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Criar_RejeitaCamposOpcionaisAcimaDosLimites()
    {
        Guid actor = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        const string document = "52998224725";

        Assert.Throws<ArgumentException>(() =>
            Fornecedor.Criar(new string('N', 201), document, null, null, null, null, actor, now));
        Assert.Throws<ArgumentException>(() =>
            Fornecedor.Criar("Fornecedor", document, new string('1', 31), null, null, null, actor, now));
        Assert.Throws<ArgumentException>(() =>
            Fornecedor.Criar("Fornecedor", document, null, $"{new string('a', 250)}@example.com", null, null, actor, now));
        Assert.Throws<ArgumentException>(() =>
            Fornecedor.Criar("Fornecedor", document, null, null, new string('C', 201), null, actor, now));
        Assert.Throws<ArgumentException>(() =>
            Fornecedor.Criar("Fornecedor", document, null, null, null, new string('O', 2001), actor, now));
    }

    [Fact]
    public void Atualizar_AtualizaDadosEAuditoria()
    {
        Fornecedor fornecedor = Fornecedor.Criar(
            "Original", "52998224725", null, null, null, null, Guid.NewGuid(), DateTimeOffset.UtcNow);
        Guid ator = Guid.NewGuid();
        DateTimeOffset agora = DateTimeOffset.Parse("2026-10-26T10:00:00-03:00");

        fornecedor.Atualizar(
            "Atualizado", "04.252.011/0001-10", " 11988887777 ", "novo@example.com",
            " João ", " Observações ", ator, agora);

        Assert.Equal("Atualizado", fornecedor.Nome);
        Assert.Equal("04252011000110", fornecedor.Documento);
        Assert.Equal("11988887777", fornecedor.Telefone);
        Assert.Equal("novo@example.com", fornecedor.Email);
        Assert.Equal(ator, fornecedor.AtualizadoPor);
        Assert.Equal(TimeSpan.Zero, fornecedor.AtualizadoEm.Offset);
    }
}
