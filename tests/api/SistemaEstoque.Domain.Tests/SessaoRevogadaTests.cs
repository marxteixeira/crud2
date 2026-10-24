using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Domain.Tests;

public sealed class SessaoRevogadaTests
{
    [Fact]
    public void Criar_ExigeIdentificadorDeSessao()
    {
        Assert.Throws<ArgumentException>(() => new SessaoRevogada(Guid.Empty, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Criar_NormalizaHorarioParaUtc()
    {
        DateTimeOffset revogadaEm = DateTimeOffset.Parse("2026-10-23T12:00:00-03:00");

        SessaoRevogada sessao = new(Guid.NewGuid(), revogadaEm);

        Assert.Equal(TimeSpan.Zero, sessao.RevogadaEm.Offset);
    }
}
