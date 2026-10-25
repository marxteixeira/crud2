using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    [Fact]
    public async Task CreateRack_ReturnsConflictForDuplicateCode()
    {
        Guid identity = Guid.NewGuid();
        FakeLocationManagementService locations = new()
        {
            CreateRackFailure = new LocationConflictException("Já existe uma estante com esse código.")
        };
        using AuthenticationTestFactory factory = new(identity, locationManagementService: locations);
        using HttpClient client = AuthenticatedClient(factory, identity);

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/locations/racks",
            new CreateRackRequest("A", null));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateRack_UnexpectedFailure_ReturnsProblemDetailsWithTraceIdAndLogsCause()
    {
        Guid identity = Guid.NewGuid();
        FakeLocationManagementService locations = new()
        {
            CreateRackFailure = new InvalidOperationException("relation \"estantes\" does not exist")
        };
        CapturingLoggerProvider provider = new();
        using AuthenticationTestFactory factory = new(identity, locationManagementService: locations);
        using WebApplicationFactory<Program> logged = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddLogging(logging => logging.AddProvider(provider))));
        using HttpClient client = logged.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Operador");

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/locations/racks",
            new CreateRackRequest("A", null));
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        using JsonDocument problem = JsonDocument.Parse(body);
        string traceId = problem.RootElement.GetProperty("traceId").GetString()!;
        Assert.DoesNotContain("estantes", body, StringComparison.Ordinal);
        (string Category, string Message, Exception? Exception) failure = Assert.Single(
            provider.Entries.Where(entry => entry.Category.EndsWith("GlobalExceptionHandler", StringComparison.Ordinal)));
        Assert.Contains(traceId, failure.Message, StringComparison.Ordinal);
        Assert.Contains("does not exist", failure.Exception?.Message, StringComparison.Ordinal);
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<(string Category, string Message, Exception? Exception)> _entries = new();

        public IReadOnlyCollection<(string Category, string Message, Exception? Exception)> Entries => _entries.ToArray();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, _entries);

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(
            string category,
            ConcurrentQueue<(string Category, string Message, Exception? Exception)> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter) =>
                entries.Enqueue((category, formatter(state, exception), exception));
        }
    }

    private static HttpClient AuthenticatedClient(AuthenticationTestFactory factory, Guid identity)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", identity.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Operador");
        return client;
    }
}
