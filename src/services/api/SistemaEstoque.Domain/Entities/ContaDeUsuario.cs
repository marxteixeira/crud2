namespace SistemaEstoque.Domain.Entities;

public enum PerfilUsuario
{
    Administrador,
    Operador
}

public enum SituacaoConta
{
    ConvitePendente,
    Ativa,
    Inativa
}

public sealed class ContaDeUsuario
{
    private ContaDeUsuario()
    {
        Email = string.Empty;
        NomeExibicao = string.Empty;
        CriadoPor = string.Empty;
        AtualizadoPor = string.Empty;
    }

    private ContaDeUsuario(
        Guid identidade,
        string email,
        string nomeExibicao,
        PerfilUsuario perfil,
        string ator,
        DateTimeOffset agora)
    {
        if (identidade == Guid.Empty)
        {
            throw new ArgumentException("A identidade do provedor é obrigatória.", nameof(identidade));
        }

        if (!Enum.IsDefined(perfil))
        {
            throw new ArgumentOutOfRangeException(nameof(perfil));
        }

        Id = Guid.NewGuid();
        Identidade = identidade;
        Email = NormalizarEmail(email);
        NomeExibicao = ValidarNome(nomeExibicao);
        Perfil = perfil;
        Situacao = SituacaoConta.ConvitePendente;
        CriadoEm = agora.ToUniversalTime();
        AtualizadoEm = CriadoEm;
        CriadoPor = ValidarAtor(ator);
        AtualizadoPor = CriadoPor;
    }

    public Guid Id { get; private set; }

    public Guid Identidade { get; private set; }

    public string Email { get; private set; }

    public string? EmailPendente { get; private set; }

    public string NomeExibicao { get; private set; }

    public PerfilUsuario Perfil { get; private set; }

    public SituacaoConta Situacao { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    public DateTimeOffset AtualizadoEm { get; private set; }

    public string CriadoPor { get; private set; }

    public string AtualizadoPor { get; private set; }

    public static ContaDeUsuario CriarConvite(
        Guid identidade,
        string email,
        string nomeExibicao,
        PerfilUsuario perfil,
        string ator,
        DateTimeOffset agora) =>
        new(identidade, email, nomeExibicao, perfil, ator, agora);

    public void Ativar(DateTimeOffset agora, string ator)
    {
        if (Situacao != SituacaoConta.ConvitePendente)
        {
            throw new InvalidOperationException("Somente convites pendentes podem ser ativados pelo titular.");
        }

        Situacao = SituacaoConta.Ativa;
        RegistrarAlteracao(agora, ator);
    }

    public void Atualizar(
        string? email,
        string? nomeExibicao,
        PerfilUsuario? perfil,
        string ator,
        DateTimeOffset agora)
    {
        if (email is not null)
        {
            EmailPendente = NormalizarEmail(email);
        }

        if (nomeExibicao is not null)
        {
            NomeExibicao = ValidarNome(nomeExibicao);
        }

        if (perfil.HasValue)
        {
            if (!Enum.IsDefined(perfil.Value))
            {
                throw new ArgumentOutOfRangeException(nameof(perfil));
            }

            Perfil = perfil.Value;
        }

        RegistrarAlteracao(agora, ator);
    }

    public void DefinirSituacao(SituacaoConta situacao, DateTimeOffset agora, string ator)
    {
        if (situacao is not (SituacaoConta.Ativa or SituacaoConta.Inativa))
        {
            throw new ArgumentOutOfRangeException(nameof(situacao));
        }

        Situacao = situacao;
        RegistrarAlteracao(agora, ator);
    }

    public void ConfirmarEmail(string emailConfirmado, DateTimeOffset agora, string ator)
    {
        string email = NormalizarEmail(emailConfirmado);
        if (!string.Equals(EmailPendente, email, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("O endereço confirmado não corresponde à troca pendente.");
        }

        Email = email;
        EmailPendente = null;
        RegistrarAlteracao(agora, ator);
    }

    private void RegistrarAlteracao(DateTimeOffset agora, string ator)
    {
        AtualizadoEm = agora.ToUniversalTime();
        AtualizadoPor = ValidarAtor(ator);
    }

    private static string NormalizarEmail(string email)
    {
        string normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length == 0 || !System.Net.Mail.MailAddress.TryCreate(normalized, out _))
        {
            throw new ArgumentException("O e-mail informado é inválido.", nameof(email));
        }

        return normalized;
    }

    private static string ValidarNome(string nomeExibicao)
    {
        if (string.IsNullOrWhiteSpace(nomeExibicao))
        {
            throw new ArgumentException("O nome de exibição é obrigatório.", nameof(nomeExibicao));
        }

        return nomeExibicao.Trim();
    }

    private static string ValidarAtor(string ator)
    {
        if (string.IsNullOrWhiteSpace(ator))
        {
            throw new ArgumentException("O ator da alteração é obrigatório.", nameof(ator));
        }

        return ator.Trim();
    }
}
