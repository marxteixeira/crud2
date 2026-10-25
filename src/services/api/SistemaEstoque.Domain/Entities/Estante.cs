using SistemaEstoque.Domain.ValueObjects;

namespace SistemaEstoque.Domain.Entities;

public sealed class Estante
{
    private Estante()
    {
        Codigo = string.Empty;
        CodigoNormalizado = string.Empty;
    }

    private Estante(string codigo, string? descricao, Guid ator, DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        Codigo = ValidarTexto(codigo, nameof(codigo));
        CodigoNormalizado = ChaveNormalizada.Criar(Codigo);
        Descricao = NormalizarDescricao(descricao);
        Situacao = SituacaoCadastro.Ativa;
        CriadoEm = agora.ToUniversalTime();
        AtualizadoEm = CriadoEm;
        CriadoPor = ValidarAtor(ator);
        AtualizadoPor = CriadoPor;
    }

    public Guid Id { get; private set; }

    public string Codigo { get; private set; }

    public string CodigoNormalizado { get; private set; }

    public string? Descricao { get; private set; }

    public SituacaoCadastro Situacao { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public Guid CriadoPor { get; private set; }

    public Guid AtualizadoPor { get; private set; }

    public ICollection<Prateleira> Prateleiras { get; } = new List<Prateleira>();

    public static Estante Criar(string codigo, string? descricao, Guid ator, DateTimeOffset agora) =>
        new(codigo, descricao, ator, agora);

    public void Atualizar(string codigo, string? descricao, Guid ator, DateTimeOffset agora)
    {
        Codigo = ValidarTexto(codigo, nameof(codigo));
        CodigoNormalizado = ChaveNormalizada.Criar(Codigo);
        Descricao = NormalizarDescricao(descricao);
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

    internal void RegistrarAlteracao(Guid ator, DateTimeOffset agora)
    {
        AtualizadoPor = ValidarAtor(ator);
        AtualizadoEm = agora.ToUniversalTime();
    }

    internal static string ValidarTexto(string valor, string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("O código é obrigatório.", nomeParametro);
        }

        return valor.Trim();
    }

    internal static Guid ValidarAtor(Guid ator) =>
        ator == Guid.Empty ? throw new ArgumentException("A conta responsável é obrigatória.", nameof(ator)) : ator;

    private static string? NormalizarDescricao(string? descricao) =>
        string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
}
