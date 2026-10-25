using System.Net;
using SistemaEstoque.API.Tests.Infrastructure;

namespace SistemaEstoque.API.Tests;

public sealed class ProductSearchEndpointsTests
{
    [Fact]
    public async Task ListProductsAtShelf_ValidatesPageSizeAndForwardsFilters()
    {
        Guid identity = Guid.NewGuid();
        Guid rackId = Guid.NewGuid();
        Guid shelfId = Guid.NewGuid();
        FakeProductManagementService products = new();
        using AuthenticationTestFactory factory = new(identity, productManagementService: products);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage invalid = await client.GetAsync(
            $"/api/locations/racks/{rackId}/shelves/{shelfId}/products?page=1&pageSize=101");
        HttpResponseMessage valid = await client.GetAsync(
            $"/api/locations/racks/{rackId}/shelves/{shelfId}/products?status=Inactive&page=2&pageSize=10");

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);
        Assert.Equal((rackId, shelfId, "Inactive", 2, 10), products.LastShelfFilters);
    }

    [Fact]
    public async Task ListProductsAtShelf_ReturnsNotFoundForUnknownShelf()
    {
        Guid identity = Guid.NewGuid();
        FakeProductManagementService products = new() { ShelfResult = null };
        using AuthenticationTestFactory factory = new(identity, productManagementService: products);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.GetAsync(
            $"/api/locations/racks/{Guid.NewGuid()}/shelves/{Guid.NewGuid()}/products");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static HttpClient AuthenticatedClient(AuthenticationTestFactory factory, Guid identity)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Operador");
        return client;
    }
}
