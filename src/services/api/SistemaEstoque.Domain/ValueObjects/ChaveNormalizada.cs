using System.Globalization;
using System.Text;

namespace SistemaEstoque.Domain.ValueObjects;

public static class ChaveNormalizada
{
    public static string Criar(string valor)
    {
        ArgumentNullException.ThrowIfNull(valor);

        string aparado = valor.Trim();
        if (aparado.Length == 0)
        {
            throw new ArgumentException("O valor é obrigatório.", nameof(valor));
        }

        string decomposto = aparado.Normalize(NormalizationForm.FormD);
        StringBuilder resultado = new(decomposto.Length);
        foreach (Rune caractere in decomposto.EnumerateRunes())
        {
            if (Rune.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(Rune.ToLowerInvariant(caractere).ToString());
            }
        }

        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }
}
