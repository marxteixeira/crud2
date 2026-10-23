namespace SistemaEstoque.API.Configuration;

public sealed class ApiOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];
}
