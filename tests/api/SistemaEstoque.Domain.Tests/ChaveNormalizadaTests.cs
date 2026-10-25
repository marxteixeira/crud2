using SistemaEstoque.Domain.ValueObjects;

namespace SistemaEstoque.Domain.Tests;

public sealed class ChaveNormalizadaTests
{
    [Theory]
    [InlineData("  Café  ", "cafe")]
    [InlineData("ESTANTE A", "estante a")]
    [InlineData("Prateleira", "prateleira")]
    [InlineData("Cafe\u0301", "cafe")]
    public void Criar_AparaRemoveDiacriticosEIgnoraCaixa(string valor, string esperado)
    {
        string resultado = ChaveNormalizada.Criar(valor);

        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_RejeitaValorVazio(string valor)
    {
        Assert.Throws<ArgumentException>(() => ChaveNormalizada.Criar(valor));
    }

    [Fact]
    public void Criar_RejeitaNulo()
    {
        Assert.Throws<ArgumentNullException>(() => ChaveNormalizada.Criar(null!));
    }
}
