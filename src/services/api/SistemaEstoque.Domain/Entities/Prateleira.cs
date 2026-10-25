using SistemaEstoque.Domain.ValueObjects;

namespace SistemaEstoque.Domain.Entities;

public sealed class Prateleira
{
    private Prateleira()
    {
        Codigo = string.Empty;
        CodigoNormalizado = string.Empty;
    }

    private Prateleira(Guid estanteId, string codigo, Guid ator, DateTimeOffset agora)
    {
        if (estanteId == Guid.Empty)
        {
            throw new ArgumentException("A estante é obrigatória.", nameof(estanteId));
        }

        Id = Guid.NewGuid();
        EstanteId = estanteId;
        Codigo = Estante.ValidarTexto(codigo, nameof(codigo));
        CodigoNormalizado = ChaveNormalizada.Criar(Codigo);
        Situacao = SituacaoCadastro.Ativa;
        CriadoEm = agora.ToUniversalTime();
        AtualizadoEm = CriadoEm;
        CriadoPor = Estante.ValidarAtor(ator);
        AtualizadoPor = CriadoPor;
    }

    public Guid Id { get; private set; }

    public Guid EstanteId { get; private set; }

    public Estante? Estante { get; private set; }

    public string Codigo { get; private set; }

    public string CodigoNormalizado { get; private set; }

    public SituacaoCadastro Situacao { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public Guid CriadoPor { get; private set; }

    public Guid AtualizadoPor { get; private set; }

    public static Prateleira Criar(Guid estanteId, string codigo, Guid ator, DateTimeOffset agora) =>
        new(estanteId, codigo, ator, agora);

    public void Atualizar(string codigo, Guid ator, DateTimeOffset agora)
    {
        Codigo = Estante.ValidarTexto(codigo, nameof(codigo));
        CodigoNormalizado = ChaveNormalizada.Criar(Codigo);
        RegistrarAlteracao(ator, agora);
    }

    public void DefinirSituacao(SituacaoCadastro situacao, Guid ator, DateTimeOffset agora)
    {
        if (!Enum.IsDefined(situacao))
        {
            throw new ArgumentOutOfRangeException(nameof(situacao));
        }

        Situacao = situacao;
        RegistrarAlteracao(ator, agora);
    }

    private void RegistrarAlteracao(Guid ator, DateTimeOffset agora)
    {
        AtualizadoPor = Estante.ValidarAtor(ator);
        AtualizadoEm = agora.ToUniversalTime();
    }
}
