using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SistemaEstoque.API.Configuration;

namespace SistemaEstoque.API.Tests;

public sealed class AuthenticationConfigurationTests
{
    [Fact]
    public void MissingSupabaseConfiguration_FailsWithoutRevealingCredentials()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();
        const string serviceRoleKey = "service-role-secret-sentinel";

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddSupabaseAuthentication(
                configuration,
                new TestHostEnvironment()));

        Assert.Contains("Supabase", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(serviceRoleKey, exception.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("not-a-url", "authenticated", "anon-key", "service-role-secret")]
    [InlineData("https://project.example", "", "anon-key", "service-role-secret")]
    [InlineData("https://project.example", "authenticated", "", "service-role-secret")]
    [InlineData("https://project.example", "authenticated", "anon-key", "")]
    public void InvalidSupabaseConfiguration_FailsWithoutEchoingSecrets(
        string configuredUrl,
        string audience,
        string anonKey,
        string serviceRoleKey)
    {
        IConfiguration configuration = CreateConfiguration(configuredUrl, audience, anonKey, serviceRoleKey);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddSupabaseAuthentication(configuration, new TestHostEnvironment()));

        Assert.Contains("Supabase", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("service-role-secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ProductionConfiguration_RequiresHttps()
    {
        IConfiguration configuration = CreateConfiguration(
            "http://project.example",
            "authenticated",
            "anon-key",
            "service-role-key");
        TestHostEnvironment environment = new() { EnvironmentName = Environments.Production };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddSupabaseAuthentication(configuration, environment));

        Assert.DoesNotContain("service-role-key", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ValidDevelopmentConfiguration_RegistersJwtAuthentication()
    {
        IConfiguration configuration = CreateConfiguration(
            "http://localhost:54321",
            "authenticated",
            "anon-key",
            "service-role-key");
        ServiceCollection services = new();

        services.AddSupabaseAuthentication(configuration, new TestHostEnvironment());

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        JwtBearerOptions options = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);
        Assert.Equal("authenticated", options.Audience);
        Assert.Equal("http://localhost:54321/auth/v1", options.Authority);
        Assert.True(options.TokenValidationParameters.RequireSignedTokens);
        Assert.True(options.TokenValidationParameters.ValidateIssuerSigningKey);
        Assert.True(options.TokenValidationParameters.ValidateIssuer);
        Assert.True(options.TokenValidationParameters.ValidateAudience);
        Assert.True(options.TokenValidationParameters.ValidateLifetime);
    }

    private static IConfiguration CreateConfiguration(
        string url,
        string audience,
        string anonKey,
        string serviceRoleKey) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Supabase:Url"] = url,
                ["Supabase:Audience"] = audience,
                ["Supabase:AnonKey"] = anonKey,
                ["Supabase:ServiceRoleKey"] = serviceRoleKey
            })
            .Build();

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = nameof(AuthenticationConfigurationTests);

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
