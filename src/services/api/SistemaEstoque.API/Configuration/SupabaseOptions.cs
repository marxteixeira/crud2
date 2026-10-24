namespace SistemaEstoque.API.Configuration;

public sealed class SupabaseOptions
{
    public const string SectionName = "Supabase";

    public string Url { get; set; } = string.Empty;

    public string AnonKey { get; set; } = string.Empty;

    public string ServiceRoleKey { get; set; } = string.Empty;

    public string Audience { get; set; } = "authenticated";

    public string Issuer => $"{Url.TrimEnd('/')}/auth/v1";
}
