namespace SistemaEstoque.Domain.ValueObjects;

public sealed record CpfCnpj
{
    private CpfCnpj(string digitos)
    {
        Digitos = digitos;
    }

    public string Digitos { get; }

    public static CpfCnpj Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("CPF ou CNPJ é obrigatório.", nameof(valor));
        }

        string digitos = new(valor.Where(caractere => caractere is >= '0' and <= '9').ToArray());
        if (valor.Any(caractere => (caractere is < '0' or > '9')
                && caractere is not '.' and not '-' and not '/' and not ' ' and not '\t'))
        {
            throw new ArgumentException("CPF ou CNPJ contém caracteres inválidos.", nameof(valor));
        }

        bool valido = digitos.Length switch
        {
            11 => ValidarCpf(digitos),
            14 => ValidarCnpj(digitos),
            _ => false
        };

        if (!valido)
        {
            throw new ArgumentException("CPF ou CNPJ inválido.", nameof(valor));
        }

        return new CpfCnpj(digitos);
    }

    private static bool ValidarCpf(string digitos)
    {
        if (digitos.All(digito => digito == digitos[0]))
        {
            return false;
        }

        int primeiro = CalcularDigito(digitos[..9], Enumerable.Range(2, 9).Reverse().ToArray());
        int segundo = CalcularDigito(digitos[..10], Enumerable.Range(2, 10).Reverse().ToArray());
        return digitos[9] - '0' == primeiro && digitos[10] - '0' == segundo;
    }

    private static bool ValidarCnpj(string digitos)
    {
        if (digitos.All(digito => digito == digitos[0]))
        {
            return false;
        }

        int primeiro = CalcularDigito(digitos[..12], [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        int segundo = CalcularDigito(digitos[..13], [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        return digitos[12] - '0' == primeiro && digitos[13] - '0' == segundo;
    }

    private static int CalcularDigito(string baseNumerica, IReadOnlyList<int> pesos)
    {
        int soma = 0;
        for (int indice = 0; indice < pesos.Count; indice++)
        {
            soma += (baseNumerica[indice] - '0') * pesos[indice];
        }

        int resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
