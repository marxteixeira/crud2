namespace SistemaEstoque.Infrastructure.Identity;

public sealed class SupabaseAdminOptions
{
    public const string SectionName = "Supabase";

    public string Url { get; set; } = string.Empty;

    public string AnonKey { get; set; } = string.Empty;

    public string ServiceRoleKey { get; set; } = string.Empty;

    public string RedirectUrl { get; set; } = string.Empty;
}
