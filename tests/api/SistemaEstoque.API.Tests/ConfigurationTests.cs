using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SistemaEstoque.Infrastructure;

namespace SistemaEstoque.API.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public void MissingConnectionString_FailsStartupWithoutRevealingSecret()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = string.Empty
            })
            .Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddInfrastructure(configuration));

        Assert.Contains("ConnectionStrings:Default", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidConnectionString_FailsStartupWithoutEchoingValue()
    {
        const string invalidValue = "Host=localhost;Port=not-a-number";
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = invalidValue
            })
            .Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddInfrastructure(configuration));

        Assert.Contains("ConnectionStrings:Default", exception.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(invalidValue, exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ProductionConnectionString_RequiresVerifiedTls()
    {
        IConfiguration configuration = CreateConfiguration(
            "Host=localhost;Database=crud2;Username=test;Password=fake-secret");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddInfrastructure(configuration, requireVerifiedTls: true));

        Assert.Contains("verified TLS", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("fake-secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ProductionConnectionString_RejectsTrustingAnyServerCertificate()
    {
        IConfiguration configuration = CreateConfiguration(
            "Host=localhost;Database=crud2;Username=test;Password=fake-secret;SSL Mode=VerifyFull;Trust Server Certificate=true");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddInfrastructure(configuration, requireVerifiedTls: true));

        Assert.Contains("verified TLS", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("fake-secret", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ProductionConnectionString_AcceptsVerifiedTls()
    {
        IConfiguration configuration = CreateConfiguration(
            "Host=localhost;Database=crud2;Username=test;Password=fake-secret;SSL Mode=VerifyFull;Trust Server Certificate=false");

        ServiceCollection services = new();
        services.AddInfrastructure(configuration, requireVerifiedTls: true);

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(SistemaEstoque.Infrastructure.Persistence.SistemaEstoqueDbContext));
    }

    [Fact]
    public void LocalConfiguration_AllowsIsolatedPostgreSqlWithoutTls()
    {
        IConfiguration configuration = CreateConfiguration(
            "Host=localhost;Database=crud2;Username=test;Password=fake-secret");

        ServiceCollection services = new();
        services.AddInfrastructure(configuration);

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(SistemaEstoque.Infrastructure.Persistence.SistemaEstoqueDbContext));
    }

    [Fact]
    public void ConnectionString_PreservesSpecialCharactersInPassword()
    {
        const string password = "p%ss#$&:word";
        IConfiguration configuration = CreateConfiguration(
            $"Host=localhost;Database=crud2;Username=test;Password={password}");

        ServiceCollection services = new();
        services.AddInfrastructure(configuration);

        NpgsqlConnectionStringBuilder parsed = new(
            configuration.GetConnectionString("Default") ?? string.Empty);
        Assert.Equal(password, parsed.Password);
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(SistemaEstoque.Infrastructure.Persistence.SistemaEstoqueDbContext));
    }

    private static IConfiguration CreateConfiguration(string connectionString) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString
            })
            .Build();
}
