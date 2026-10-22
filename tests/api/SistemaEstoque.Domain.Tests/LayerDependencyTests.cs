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
    public void Application_ReferencesOnlyDomainWhenUsed()
    {
        Assembly app = Assembly.Load("SistemaEstoque.Application");
        Assert.DoesNotContain(app.GetReferencedAssemblies(),
            a => a.Name == "SistemaEstoque.Infrastructure" || a.Name == "SistemaEstoque.API");
    }
}
