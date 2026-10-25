using System.Net;
using System.Net.Http.Json;
using SistemaEstoque.API.Tests.Infrastructure;
using SistemaEstoque.Application.Products;

namespace SistemaEstoque.API.Tests;

public sealed class ProductEndpointsTests
{
    [Fact]
    public async Task CreateProduct_OperatorCannotSetPrice()
    {
        Guid identity = Guid.NewGuid();
        FakeProductManagementService products = new();
        using AuthenticationTestFactory factory = new(identity, productManagementService: products);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/products", new CreateProductRequest("Café", 3, 12.50m, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(products.LastCreateRequest);
    }

    [Fact]
    public async Task CreateProduct_OperatorMayCreateWithoutPriceAndActorIsRecorded()
    {
        Guid identity = Guid.NewGuid();
        FakeProductManagementService products = new();
        using AuthenticationTestFactory factory = new(identity, productManagementService: products);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/products", new CreateProductRequest("Café", 3, null, null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(identity, products.LastActor);
        Assert.False(products.LastIsAdministrator);
        Assert.Null(products.LastCreateRequest?.Price);
    }

    [Fact]
    public async Task CreateProduct_AdministratorMaySetPrice()
    {
        Guid identity = Guid.NewGuid();
        FakeProductManagementService products = new();
        using AuthenticationTestFactory factory = new(identity, productManagementService: products);
        using HttpClient client = AuthenticatedClient(factory, identity, "Administrador");

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/products", new CreateProductRequest("Café", 3, 12.50m, null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(products.LastIsAdministrator);
        Assert.Equal(12.50m, products.LastCreateRequest?.Price);
    }

    [Fact]
    public async Task UpdateProduct_OperatorCannotChangePrice()
    {
        Guid identity = Guid.NewGuid();
        FakeProductManagementService products = new();
        using AuthenticationTestFactory factory = new(identity, productManagementService: products);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.PatchAsJsonAsync(
            $"/api/products/{Guid.NewGuid()}", new UpdateProductRequest(null, null, 12.50m, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListProducts_RejectsInvalidPagination()
    {
        Guid identity = Guid.NewGuid();
        using AuthenticationTestFactory factory = new(identity);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.GetAsync("/api/products?page=0&pageSize=25");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListProducts_ForwardsCombinedFiltersAndPagination()
    {
        Guid identity = Guid.NewGuid();
        Guid rackId = Guid.NewGuid();
        Guid shelfId = Guid.NewGuid();
        FakeProductManagementService products = new();
        using AuthenticationTestFactory factory = new(identity, productManagementService: products);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.GetAsync(
            $"/api/products?search=cafe&rackId={rackId}&shelfId={shelfId}&status=Active&page=2&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(("cafe", rackId, shelfId, "Active", 2, 10), products.LastFilters);
    }

    [Fact]
    public async Task SetProductStatus_OperatorCannotActivate()
    {
        Guid identity = Guid.NewGuid();
        FakeProductManagementService products = new();
        using AuthenticationTestFactory factory = new(identity, productManagementService: products);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/products/{Guid.NewGuid()}/status", new SetProductStatusRequest("Active"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(products.LastActor);
    }

    private static HttpClient AuthenticatedClient(
        AuthenticationTestFactory factory, Guid identity, string role)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }
}
