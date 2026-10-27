using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SistemaEstoque.Application.Users;

namespace SistemaEstoque.Infrastructure.Identity;

public sealed class SupabaseAdminClient(
    HttpClient httpClient,
    IConfiguration configuration) : IIdentityAdministration
{
    public async Task<Guid> InviteAsync(
        string email,
        string displayName,
        CancellationToken cancellationToken)
    {
        SupabaseAdminOptions options = ReadOptions();
        string inviteEndpoint = "auth/v1/invite";
        if (!string.IsNullOrWhiteSpace(options.RedirectUrl))
        {
            string confirmUrl = options.RedirectUrl.TrimEnd('/') + "/auth/confirm";
            inviteEndpoint += "?redirect_to=" + Uri.EscapeDataString(confirmUrl);
        }

        using HttpRequestMessage request = CreateAdminRequest(
            HttpMethod.Post,
            new Uri(new Uri(options.Url.TrimEnd('/') + "/"), inviteEndpoint),
            options.ServiceRoleKey);
        request.Content = JsonContent.Create(new
        {
            email,
            data = new { display_name = displayName }
        });

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        EnsureSuccess(response);
        using JsonDocument document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);
        if (!Guid.TryParse(document.RootElement.GetProperty("id").GetString(), out Guid identity))
        {
            throw new InvalidOperationException("O provedor retornou uma identidade inválida.");
        }

        return identity;
    }

    public async Task DeleteAsync(Guid identity, CancellationToken cancellationToken)
    {
        SupabaseAdminOptions options = ReadOptions();
        using HttpRequestMessage request = CreateAdminRequest(
            HttpMethod.Delete,
            new Uri(new Uri(options.Url.TrimEnd('/') + "/"), $"auth/v1/admin/users/{identity}"),
            options.ServiceRoleKey);
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        EnsureSuccess(response);
    }

    public async Task UpdateRoleAsync(
        Guid identity,
        string role,
        CancellationToken cancellationToken)
    {
        SupabaseAdminOptions options = ReadOptions();
        using HttpRequestMessage request = CreateAdminRequest(
            HttpMethod.Put,
            new Uri(new Uri(options.Url.TrimEnd('/') + "/"), $"auth/v1/admin/users/{identity}"),
            options.ServiceRoleKey);
        request.Content = JsonContent.Create(new { app_metadata = new { role } });
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        EnsureSuccess(response);
    }

    public async Task RequestEmailChangeAsync(
        Guid identity,
        string email,
        CancellationToken cancellationToken)
    {
        SupabaseAdminOptions options = ReadOptions();
        using HttpRequestMessage request = CreateAdminRequest(
            HttpMethod.Put,
            new Uri(new Uri(options.Url.TrimEnd('/') + "/"), $"auth/v1/admin/users/{identity}"),
            options.ServiceRoleKey);
        request.Content = JsonContent.Create(new { email, email_confirm = false });
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        EnsureSuccess(response);
    }

    public async Task<string?> GetConfirmedEmailAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        SupabaseAdminOptions options = ReadOptions();
        if (string.IsNullOrWhiteSpace(options.AnonKey))
        {
            throw new InvalidOperationException("A configuração Supabase:AnonKey é obrigatória.");
        }

        using HttpRequestMessage request = CreateAdminRequest(
            HttpMethod.Get,
            new Uri(new Uri(options.Url.TrimEnd('/') + "/"), "auth/v1/user"),
            options.AnonKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        EnsureSuccess(response);
        using JsonDocument document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);
        string? confirmedAt = document.RootElement.TryGetProperty("email_confirmed_at", out JsonElement confirmation)
            ? confirmation.GetString()
            : null;
        return string.IsNullOrWhiteSpace(confirmedAt)
            ? null
            : document.RootElement.GetProperty("email").GetString();
    }

    private SupabaseAdminOptions ReadOptions()
    {
        SupabaseAdminOptions options = configuration.GetSection(SupabaseAdminOptions.SectionName)
            .Get<SupabaseAdminOptions>()
            ?? throw new InvalidOperationException("Required configuration section 'Supabase' is missing.");
        if (!Uri.TryCreate(options.Url, UriKind.Absolute, out Uri? uri)
            || uri.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException("Configuration 'Supabase:Url' is invalid.");
        }

        return options;
    }

    private static HttpRequestMessage CreateAdminRequest(HttpMethod method, Uri uri, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("The required Supabase API key is not configured.");
        }

        HttpRequestMessage request = new(method, uri);
        request.Headers.Add("apikey", key);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        return request;
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Supabase request failed with HTTP {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }
    }
}
