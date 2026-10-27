using System.Net;
using System.Net.Http.Json;
using SistemaEstoque.API.Tests.Infrastructure;
using SistemaEstoque.Application.Suppliers;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.API.Tests;

public sealed class SupplierEndpointsTests
{
    [Fact]
    public async Task List_RequiresAuthenticatedAccount()
    {
        using AuthenticationTestFactory factory = new(Guid.NewGuid());
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/suppliers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_UsesSearchStatusAndPagination()
    {
        Guid identity = Guid.NewGuid();
        FakeSupplierManagementService service = new();
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.GetAsync(
            "/api/suppliers?search=loja&status=Active&page=2&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(("loja", "Active", 2, 10), service.LastFilters);
    }

    [Fact]
    public async Task Get_ReturnsSupplierOrNotFound()
    {
        Guid identity = Guid.NewGuid();
        Guid supplierId = Guid.NewGuid();
        SupplierDto supplier = new(
            supplierId, "Fornecedor", "52998224725", null, null, null, null, "Active",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, identity, identity);
        FakeSupplierManagementService service = new() { Supplier = supplier };
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.GetAsync($"/api/suppliers/{supplierId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        response = await client.GetAsync($"/api/suppliers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("?page=0")]
    [InlineData("?pageSize=101")]
    [InlineData("?status=Unknown")]
    public async Task List_RejectsInvalidQuery(string query)
    {
        Guid identity = Guid.NewGuid();
        using AuthenticationTestFactory factory = new(identity);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.GetAsync($"/api/suppliers{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_RecordsAuthenticatedActor()
    {
        Guid identity = Guid.NewGuid();
        FakeSupplierManagementService service = new();
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/suppliers",
            new CreateSupplierRequest("Fornecedor", "52998224725", null, null, null, null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(identity, service.LastActor);
    }

    [Fact]
    public async Task Create_RejectsInvalidCpfBeforeCallingService()
    {
        Guid identity = Guid.NewGuid();
        FakeSupplierManagementService service = new();
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/suppliers",
            new CreateSupplierRequest("Fornecedor", "52998224724", null, null, null, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(service.LastCreateRequest);
    }

    [Fact]
    public async Task Create_RejectsInvalidEmailAndReturnsValidationProblemDetails()
    {
        Guid identity = Guid.NewGuid();
        FakeSupplierManagementService service = new();
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/suppliers",
            new CreateSupplierRequest("Fornecedor", "52998224725", null, "email-invalido", null, null));
        string problem = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("errors", problem, StringComparison.OrdinalIgnoreCase);
        Assert.Null(service.LastCreateRequest);
    }

    [Fact]
    public async Task Create_ReturnsConflictForDuplicateDocument()
    {
        Guid identity = Guid.NewGuid();
        FakeSupplierManagementService service = new()
        {
            CreateFailure = new SupplierConflictException("Já existe um fornecedor com esse CPF ou CNPJ.")
        };
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/suppliers",
            new CreateSupplierRequest("Fornecedor", "52998224725", null, null, null, null));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_RejectsOperator()
    {
        Guid identity = Guid.NewGuid();
        FakeSupplierManagementService service = new();
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/suppliers",
            new CreateSupplierRequest("Fornecedor", "52998224725", null, null, null, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(service.LastCreateRequest);
    }

    [Fact]
    public async Task SetStatus_ReturnsConflictWhenSupplierHasActiveProducts()
    {
        Guid identity = Guid.NewGuid();
        Guid supplierId = Guid.NewGuid();
        FakeSupplierManagementService service = new()
        {
            StatusFailure = new SupplierConflictException(
                "Não é possível inativar o fornecedor enquanto houver produtos ativos vinculados.")
        };
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/suppliers/{supplierId}/status",
            new SetSupplierStatusRequest("Inactive"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_RecordsAuthenticatedActorAndFullRequest()
    {
        Guid identity = Guid.NewGuid();
        Guid supplierId = Guid.NewGuid();
        SupplierDto supplier = new(
            supplierId, "Fornecedor atualizado", "52998224725", null, null, null, null, "Active",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, identity, identity);
        FakeSupplierManagementService service = new() { Supplier = supplier };
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.PatchAsJsonAsync(
            $"/api/suppliers/{supplierId}",
            new UpdateSupplierRequest("Fornecedor atualizado", "52998224725", null, null, null, null));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(identity, service.LastActor);
        Assert.Equal("Fornecedor atualizado", service.LastUpdateRequest?.Name);
    }

    [Fact]
    public async Task Update_RejectsOperatorBeforeCallingService()
    {
        Guid identity = Guid.NewGuid();
        FakeSupplierManagementService service = new();
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.PatchAsJsonAsync(
            $"/api/suppliers/{Guid.NewGuid()}",
            new UpdateSupplierRequest("Fornecedor", "52998224725", null, null, null, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(service.LastUpdateRequest);
    }

    [Fact]
    public async Task SetStatus_RecordsAdministratorAndRequestedStatus()
    {
        Guid identity = Guid.NewGuid();
        FakeSupplierManagementService service = new();
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/suppliers/{Guid.NewGuid()}/status",
            new SetSupplierStatusRequest("Inactive"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(identity, service.LastActor);
        Assert.Equal(SituacaoCadastro.Inativa, service.LastStatus);
    }

    [Fact]
    public async Task ListProductsForSupplier_ForwardsFiltersAndPagination()
    {
        Guid identity = Guid.NewGuid();
        Guid supplierId = Guid.NewGuid();
        FakeSupplierManagementService service = new();
        using AuthenticationTestFactory factory = new(identity, supplierManagementService: service);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.GetAsync(
            $"/api/suppliers/{supplierId}/products?status=Inactive&linkStatus=Inactive&page=2&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal((supplierId, "Inactive", "Inactive", 2, 10), service.LastProductFilters);
    }

    [Fact]
    public async Task AssociateSupplier_UsesAuthenticatedActor()
    {
        Guid identity = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        Guid supplierId = Guid.NewGuid();
        FakeProductSupplierService links = new();
        using AuthenticationTestFactory factory = new(identity, productSupplierService: links);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.PutAsync(
            $"/api/products/{productId}/suppliers/{supplierId}", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(identity, links.LastActor);
        Assert.Equal((productId, supplierId), links.LastPair);
    }

    [Fact]
    public async Task AssociateSupplier_ReturnsConflictForInactiveProductOrSupplier()
    {
        Guid identity = Guid.NewGuid();
        FakeProductSupplierService links = new()
        {
            AssociateFailure = new ProductSupplierConflictException(
                "Somente produtos e fornecedores ativos podem ser associados.")
        };
        using AuthenticationTestFactory factory = new(identity, productSupplierService: links);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.PutAsync(
            $"/api/products/{Guid.NewGuid()}/suppliers/{Guid.NewGuid()}", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ListSuppliersForProduct_ForwardsHistoryAndPaginationFilters()
    {
        Guid identity = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        FakeProductSupplierService links = new();
        using AuthenticationTestFactory factory = new(identity, productSupplierService: links);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.GetAsync(
            $"/api/products/{productId}/suppliers?linkStatus=Inactive&page=3&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal((productId, "Inactive", 3, 5), links.LastFilters);
    }

    [Fact]
    public async Task DisassociateSupplier_RecordsActorAndReturnsNotFoundWhenLinkIsAbsent()
    {
        Guid identity = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        Guid supplierId = Guid.NewGuid();
        FakeProductSupplierService links = new();
        using AuthenticationTestFactory factory = new(identity, productSupplierService: links);
        using HttpClient client = AuthenticatedClient(factory, identity, "Operador");

        HttpResponseMessage response = await client.DeleteAsync(
            $"/api/products/{productId}/suppliers/{supplierId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(identity, links.LastActor);
        Assert.Equal((productId, supplierId), links.LastPair);

        links.DisassociateFailure = new KeyNotFoundException();
        response = await client.DeleteAsync($"/api/products/{productId}/suppliers/{supplierId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static HttpClient AuthenticatedClient(
        AuthenticationTestFactory factory,
        Guid identity,
        string role = "Administrador")
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }
}
