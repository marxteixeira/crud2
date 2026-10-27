using SistemaEstoque.Domain.ValueObjects;

namespace SistemaEstoque.Domain.Tests;

public sealed class CpfCnpjTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("04.252.011/0001-10", "04252011000110")]
    public void Criar_NormalizaDocumentoValido(string entrada, string esperado)
    {
        CpfCnpj documento = CpfCnpj.Criar(entrada);

        Assert.Equal(esperado, documento.Digitos);
    }

    [Theory]
    [InlineData("52998224724")]
    [InlineData("04252011000111")]
    [InlineData("111.111.111-11")]
    [InlineData("00000000000000")]
    [InlineData("123")]
    [InlineData("abc")]
    public void Criar_RejeitaDocumentoInvalido(string entrada)
    {
        Assert.Throws<ArgumentException>(() => CpfCnpj.Criar(entrada));
    }

    [Fact]
    public void Criar_RejeitaEntradaVazia()
    {
        Assert.Throws<ArgumentException>(() => CpfCnpj.Criar(" "));
    }
}
