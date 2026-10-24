using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaEstoque.Infrastructure;

namespace SistemaEstoque.API.Tests;

public sealed class SecretConfigurationTests
{
    [Fact]
    public void InvalidConnectionString_DoesNotExposeSuppliedPassword()
    {
        const string secret = "test-only-password-sentinel";
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] =
                    $"Host=localhost;Port=not-a-number;Password={secret}"
            })
            .Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => new ServiceCollection().AddInfrastructure(configuration));

        Assert.Contains("ConnectionStrings:Default", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(secret, exception.ToString(), StringComparison.Ordinal);
    }
}
