using SistemaEstoque.Domain.ValueObjects;

namespace SistemaEstoque.Domain.Entities;

public sealed class Produto
{
    private Produto()
    {
        Nome = string.Empty;
        NomeNormalizado = string.Empty;
    }

    private Produto(
        string nome,
        int quantidade,
        decimal? preco,
        Guid? prateleiraId,
        SituacaoCadastro situacao,
        Guid ator,
        DateTimeOffset agora)
    {
        if (quantidade < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade não pode ser negativa.");
        }

        Id = Guid.NewGuid();
        Nome = ValidarNome(nome);
        NomeNormalizado = ChaveNormalizada.Criar(Nome);
        Quantidade = quantidade;
        Preco = ValidarPreco(preco);
        PrateleiraId = prateleiraId;
        Situacao = situacao;
        CriadoEm = agora.ToUniversalTime();
        AtualizadoEm = CriadoEm;
        CriadoPor = Estante.ValidarAtor(ator);
        AtualizadoPor = CriadoPor;
        if (Situacao == SituacaoCadastro.Ativa && (Preco is null || PrateleiraId is null))
        {
            throw new InvalidOperationException("Produto ativo exige preço e localização válidos.");
        }
    }

    public Guid Id { get; private set; }

    public string Nome { get; private set; }

    public string NomeNormalizado { get; private set; }

    public int Quantidade { get; private set; }

    public decimal? Preco { get; private set; }

    public Guid? PrateleiraId { get; private set; }

    public Prateleira? Prateleira { get; private set; }

    public SituacaoCadastro Situacao { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public Guid CriadoPor { get; private set; }

    public Guid AtualizadoPor { get; private set; }

    public static Produto Criar(
        string nome,
        int quantidade,
        decimal? preco,
        Guid? prateleiraId,
        SituacaoCadastro situacao,
        Guid ator,
        DateTimeOffset agora) =>
        new(nome, quantidade, preco, prateleiraId, situacao, ator, agora);

    public void Atualizar(
        string? nome,
        int? quantidade,
        decimal? preco,
        Guid? prateleiraId,
        bool localizacaoValida,
        Guid ator,
        DateTimeOffset agora)
    {
        string nomeFinal = nome is null ? Nome : ValidarNome(nome);
        int quantidadeFinal = quantidade ?? Quantidade;
        decimal? precoFinal = preco is null ? Preco : ValidarPreco(preco);
        Guid? prateleiraFinal = prateleiraId ?? PrateleiraId;

        if (quantidadeFinal < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade não pode ser negativa.");
        }

        ValidarProdutoAtivo(
            Situacao == SituacaoCadastro.Ativa,
            localizacaoValida,
            precoFinal,
            prateleiraFinal);
        Nome = nomeFinal;
        NomeNormalizado = ChaveNormalizada.Criar(nomeFinal);
        Quantidade = quantidadeFinal;
        Preco = precoFinal;
        PrateleiraId = prateleiraFinal;
        RegistrarAlteracao(ator, agora);
    }

    public void DefinirSituacao(
        SituacaoCadastro situacao,
        bool localizacaoValida,
        Guid ator,
        DateTimeOffset agora)
    {
        if (!Enum.IsDefined(situacao))
        {
            throw new ArgumentOutOfRangeException(nameof(situacao));
        }

        if (situacao == SituacaoCadastro.Ativa)
        {
            ValidarProdutoAtivo(true, localizacaoValida, Preco, PrateleiraId);
        }

        Situacao = situacao;
        RegistrarAlteracao(ator, agora);
    }

    private static void ValidarProdutoAtivo(
        bool deveFicarAtivo,
        bool localizacaoValida,
        decimal? preco,
        Guid? prateleiraId)
    {
        if (deveFicarAtivo && (preco is null || prateleiraId is null || !localizacaoValida))
        {
            throw new InvalidOperationException("Produto ativo exige preço e localização válidos.");
        }
    }

    private void RegistrarAlteracao(Guid ator, DateTimeOffset agora)
    {
        AtualizadoPor = Estante.ValidarAtor(ator);
        AtualizadoEm = agora.ToUniversalTime();
    }

    private static string ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("O nome do produto é obrigatório.", nameof(nome));
        }

        return nome.Trim();
    }

    private static decimal? ValidarPreco(decimal? preco)
    {
        if (preco is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(preco), "O preço não pode ser negativo.");
        }

        if (preco.HasValue && decimal.Round(preco.Value, 2) != preco.Value)
        {
            throw new ArgumentOutOfRangeException(nameof(preco), "O preço deve ter no máximo duas casas decimais.");
        }

        return preco;
    }
}
