using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
}
