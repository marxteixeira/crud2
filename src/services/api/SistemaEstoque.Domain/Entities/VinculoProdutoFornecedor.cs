namespace SistemaEstoque.Domain.Entities;

public sealed class VinculoProdutoFornecedor
{
    private VinculoProdutoFornecedor()
    {
    }

    private VinculoProdutoFornecedor(
        Guid produtoId,
        Guid fornecedorId,
        Guid ator,
        DateTimeOffset agora)
    {
        if (produtoId == Guid.Empty)
        {
            throw new ArgumentException("O produto é obrigatório.", nameof(produtoId));
        }

        if (fornecedorId == Guid.Empty)
        {
            throw new ArgumentException("O fornecedor é obrigatório.", nameof(fornecedorId));
        }

        Id = Guid.NewGuid();
        ProdutoId = produtoId;
        FornecedorId = fornecedorId;
        Situacao = SituacaoCadastro.Ativa;
        CriadoEm = agora.ToUniversalTime();
        AtualizadoEm = CriadoEm;
        CriadoPor = Estante.ValidarAtor(ator);
        AtualizadoPor = CriadoPor;
    }

    public Guid Id { get; private set; }

    public Guid ProdutoId { get; private set; }

    public Produto Produto { get; private set; } = null!;

    public Guid FornecedorId { get; private set; }

    public Fornecedor Fornecedor { get; private set; } = null!;

    public SituacaoCadastro Situacao { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public Guid CriadoPor { get; private set; }

    public Guid AtualizadoPor { get; private set; }

    public static VinculoProdutoFornecedor Criar(
        Guid produtoId,
        Guid fornecedorId,
        Guid ator,
        DateTimeOffset agora) =>
        new(produtoId, fornecedorId, ator, agora);

    public void DefinirSituacao(SituacaoCadastro situacao, Guid ator, DateTimeOffset agora)
    {
        if (!Enum.IsDefined(situacao))
        {
            throw new ArgumentOutOfRangeException(nameof(situacao));
        }

        Situacao = situacao;
        AtualizadoPor = Estante.ValidarAtor(ator);
        AtualizadoEm = agora.ToUniversalTime();
    }
}
