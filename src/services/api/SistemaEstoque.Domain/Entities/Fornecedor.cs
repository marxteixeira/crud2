using System.Net.Mail;
using SistemaEstoque.Domain.ValueObjects;

namespace SistemaEstoque.Domain.Entities;

public sealed class Fornecedor
{
    private Fornecedor()
    {
        Nome = string.Empty;
        Documento = string.Empty;
    }

    private Fornecedor(
        string nome,
        string documento,
        string? telefone,
        string? email,
        string? nomeContato,
        string? observacoes,
        Guid ator,
        DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        Nome = ValidarTexto(nome, nameof(nome), 200, "O nome ou razão social é obrigatório.");
        Documento = CpfCnpj.Criar(documento).Digitos;
        Telefone = ValidarOpcional(telefone, nameof(telefone), 30);
        Email = ValidarEmail(email);
        NomeContato = ValidarOpcional(nomeContato, nameof(nomeContato), 200);
        Observacoes = ValidarOpcional(observacoes, nameof(observacoes), 2000);
        Situacao = SituacaoCadastro.Ativa;
        CriadoEm = agora.ToUniversalTime();
        AtualizadoEm = CriadoEm;
        CriadoPor = Estante.ValidarAtor(ator);
        AtualizadoPor = CriadoPor;
    }

    public Guid Id { get; private set; }

    public string Nome { get; private set; }

    public string Documento { get; private set; }

    public string? Telefone { get; private set; }

    public string? Email { get; private set; }

    public string? NomeContato { get; private set; }

    public string? Observacoes { get; private set; }

    public SituacaoCadastro Situacao { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public Guid CriadoPor { get; private set; }

    public Guid AtualizadoPor { get; private set; }

    public ICollection<VinculoProdutoFornecedor> Vinculos { get; } = new List<VinculoProdutoFornecedor>();

    public static Fornecedor Criar(
        string nome,
        string documento,
        string? telefone,
        string? email,
        string? nomeContato,
        string? observacoes,
        Guid ator,
        DateTimeOffset agora) =>
        new(nome, documento, telefone, email, nomeContato, observacoes, ator, agora);

    public void Atualizar(
        string nome,
        string documento,
        string? telefone,
        string? email,
        string? nomeContato,
        string? observacoes,
        Guid ator,
        DateTimeOffset agora)
    {
        string nomeFinal = ValidarTexto(nome, nameof(nome), 200, "O nome ou razão social é obrigatório.");
        string documentoFinal = CpfCnpj.Criar(documento).Digitos;
        string? telefoneFinal = ValidarOpcional(telefone, nameof(telefone), 30);
        string? emailFinal = ValidarEmail(email);
        string? nomeContatoFinal = ValidarOpcional(nomeContato, nameof(nomeContato), 200);
        string? observacoesFinal = ValidarOpcional(observacoes, nameof(observacoes), 2000);

        Nome = nomeFinal;
        Documento = documentoFinal;
        Telefone = telefoneFinal;
        Email = emailFinal;
        NomeContato = nomeContatoFinal;
        Observacoes = observacoesFinal;
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

    private static string ValidarTexto(string valor, string parametro, int maximo, string mensagem)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException(mensagem, parametro);
        }

        string normalizado = valor.Trim();
        if (normalizado.Length > maximo)
        {
            throw new ArgumentException($"O campo deve ter no máximo {maximo} caracteres.", parametro);
        }

        return normalizado;
    }

    private static string? ValidarOpcional(string? valor, string parametro, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        string normalizado = valor.Trim();
        if (normalizado.Length > maximo)
        {
            throw new ArgumentException($"O campo deve ter no máximo {maximo} caracteres.", parametro);
        }

        return normalizado;
    }

    private static string? ValidarEmail(string? email)
    {
        string? normalizado = ValidarOpcional(email, nameof(email), 254);
        if (normalizado is null)
        {
            return null;
        }

        try
        {
            MailAddress endereco = new(normalizado);
            if (!string.Equals(endereco.Address, normalizado, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException();
            }
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("O e-mail informado é inválido.", nameof(email), exception);
        }

        return normalizado;
    }
}
