namespace SistemaEstoque.Domain.Entities;

public sealed class SessaoRevogada
{
    private SessaoRevogada()
    {
    }

    public SessaoRevogada(Guid identificador, DateTimeOffset revogadaEm)
    {
        if (identificador == Guid.Empty)
        {
            throw new ArgumentException("O identificador de sessão é obrigatório.", nameof(identificador));
        }

        Identificador = identificador;
        RevogadaEm = revogadaEm.ToUniversalTime();
    }

    public Guid Identificador { get; private set; }

    public DateTimeOffset RevogadaEm { get; private set; }
}
