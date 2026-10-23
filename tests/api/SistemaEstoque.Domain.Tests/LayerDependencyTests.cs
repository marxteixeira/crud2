using System.Reflection;

namespace SistemaEstoque.Domain.Tests;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_DoesNotReferenceOtherLayers()
    {
        Assembly domain = Assembly.Load("SistemaEstoque.Domain");
        Assert.Empty(domain.GetReferencedAssemblies().Where(a => a.Name!.StartsWith("SistemaEstoque.")));
    }

    [Fact]
    public void Domain_DoesNotReferenceExternalPackages()
    {
        Assembly domain = Assembly.Load("SistemaEstoque.Domain");
        string[] externalReferences = domain.GetReferencedAssemblies()
            .Where(assembly =>
                assembly.Name is not ("System" or "netstandard")
                && assembly.Name?.StartsWith("System.", StringComparison.Ordinal) is false
                && assembly.Name?.StartsWith("Microsoft.", StringComparison.Ordinal) is false)
            .Select(assembly => assembly.Name!)
            .ToArray();

        Assert.Empty(externalReferences);
    }

    [Fact]
    public void Application_ReferencesDomainButNotOuterLayers()
    {
        Assembly app = Assembly.Load("SistemaEstoque.Application");
        Assert.DoesNotContain(app.GetReferencedAssemblies(),
            a => a.Name == "SistemaEstoque.Infrastructure" || a.Name == "SistemaEstoque.API");
    }
}
