using System.Net;
using System.Net.Http.Json;
using SistemaEstoque.API.Tests.Infrastructure;
using SistemaEstoque.Application.Locations;

namespace SistemaEstoque.API.Tests;

public sealed class LocationEndpointsTests
{
    [Fact]
    public async Task ListRacks_RequiresActiveAuthenticatedAccount()
    {
        using AuthenticationTestFactory factory =
            new(Guid.NewGuid(), locationManagementService: new FakeLocationManagementService());
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/locations/racks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListRacks_ReturnsRacksForAuthenticatedAccount()
    {
        Guid identity = Guid.NewGuid();
        FakeLocationManagementService locations = new()
        {
            Racks = [new RackDto(Guid.NewGuid(), "A", null, "Active", [])]
        };
        using AuthenticationTestFactory factory = new(identity, locationManagementService: locations);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.GetAsync("/api/locations/racks?status=Active");
        RackDto[]? racks = await response.Content.ReadFromJsonAsync<RackDto[]>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(racks);
        Assert.Single(racks);
        Assert.Equal("A", racks[0].Code);
    }

    [Fact]
    public async Task ListRacks_RejectsUnknownStatus()
    {
        Guid identity = Guid.NewGuid();
        using AuthenticationTestFactory factory =
            new(identity, locationManagementService: new FakeLocationManagementService());
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.GetAsync("/api/locations/racks?status=Unknown");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRack_RecordsAuthenticatedActor()
    {
        Guid identity = Guid.NewGuid();
        FakeLocationManagementService locations = new();
        using AuthenticationTestFactory factory = new(identity, locationManagementService: locations);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/locations/racks",
            new CreateRackRequest("A", "Estoque"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(identity, locations.LastActor);
    }

    [Fact]
    public async Task CreateRack_RejectsEmptyCode()
    {
        Guid identity = Guid.NewGuid();
        FakeLocationManagementService locations = new();
        using AuthenticationTestFactory factory = new(identity, locationManagementService: locations);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/locations/racks",
            new CreateRackRequest(" ", null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(locations.Racks);
    }

    private static HttpClient AuthenticatedClient(AuthenticationTestFactory factory, Guid identity)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Operador");
        return client;
    }
}
