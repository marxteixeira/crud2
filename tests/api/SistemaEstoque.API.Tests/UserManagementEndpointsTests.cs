using System.Net;
using System.Net.Http.Json;
using SistemaEstoque.API.Tests.Infrastructure;
using SistemaEstoque.Application.Users;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.API.Tests;

public sealed class UserManagementEndpointsTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task List_RequiresAdministratorPolicy()
    {
        FakeUserManagementService service = new();
        using AuthenticationTestFactory factory =
            new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Operador");

        using HttpResponseMessage response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, service.ListCalls);
    }

    [Fact]
    public async Task List_UsesValidatedFiltersAndReturnsPage()
    {
        FakeUserManagementService service = new();
        using AuthenticationTestFactory factory =
            new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", Guid.NewGuid().ToString());

        using HttpResponseMessage response =
            await client.GetAsync("/api/users?search=ana&status=Ativa&page=2&pageSize=10");
        PaginatedUsersDto? result = await response.Content.ReadFromJsonAsync<PaginatedUsersDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ana", service.Search);
        Assert.Equal("Ativa", service.Status);
        Assert.Equal(2, service.Page);
        Assert.Equal(10, service.PageSize);
        Assert.Equal(2, result?.Page);
    }

    [Fact]
    public async Task List_InvalidPagination_ReturnsValidationProblem()
    {
        FakeUserManagementService service = new();
        using AuthenticationTestFactory factory =
            new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", Guid.NewGuid().ToString());

        using HttpResponseMessage response = await client.GetAsync("/api/users?page=0&pageSize=101");
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("errors", body, StringComparison.Ordinal);
        Assert.Equal(0, service.ListCalls);
    }

    [Fact]
    public async Task Invite_ReturnsCreatedAccount()
    {
        FakeUserManagementService service = new();
        using AuthenticationTestFactory factory =
            new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        Guid actor = Guid.NewGuid();
        client.DefaultRequestHeaders.Add("X-Test-Subject", actor.ToString());

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/users",
            new CreateUserRequest("new@example.com", "New User", "Operador"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(actor.ToString(), service.Actor);
        Assert.Equal("new@example.com", service.LastCreated?.Email);
    }

    [Fact]
    public async Task Invite_DuplicateEmailReturnsProblemDetailsConflict()
    {
        FakeUserManagementService service = new() { ConflictOnInvite = true };
        using AuthenticationTestFactory factory =
            new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", Guid.NewGuid().ToString());

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/users",
            new CreateUserRequest("duplicate@example.com", "Duplicate", "Operador"));
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("\"status\":409", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Get_ReturnsSelectedAccount()
    {
        FakeUserManagementService service = new();
        using AuthenticationTestFactory factory =
            new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", Guid.NewGuid().ToString());

        using HttpResponseMessage response = await client.GetAsync($"/api/users/{UserId}");
        UserAccountDto? result = await response.Content.ReadFromJsonAsync<UserAccountDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(UserId, service.LastFoundId);
        Assert.Equal(UserId, result?.Id);
    }

    [Fact]
    public async Task Update_ReturnsUpdatedAccount()
    {
        FakeUserManagementService service = new();
        using AuthenticationTestFactory factory =
            new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", Guid.NewGuid().ToString());

        using HttpResponseMessage response = await client.PatchAsJsonAsync(
            $"/api/users/{UserId}",
            new UpdateUserRequest(null, "Updated Name", "Administrador"));
        UserAccountDto? result = await response.Content.ReadFromJsonAsync<UserAccountDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(UserId, service.LastUpdatedId);
        Assert.Equal("Updated Name", service.LastUpdate?.DisplayName);
        Assert.Equal("Updated Name", result?.DisplayName);
    }

    [Fact]
    public async Task SetActivation_ChangesAccountStatus()
    {
        FakeUserManagementService service = new();
        using AuthenticationTestFactory factory =
            new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", Guid.NewGuid().ToString());

        using HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/users/{UserId}/activation",
            new { status = "Inativa" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(UserId, service.LastStatusId);
        Assert.Equal(SituacaoConta.Inativa, service.LastStatus);
    }

    [Fact]
    public async Task ResendInvitation_ReturnsNoContentForAdministrator()
    {
        FakeUserManagementService service = new();
        using AuthenticationTestFactory factory = new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", Guid.NewGuid().ToString());

        using HttpResponseMessage response = await client.PostAsync($"/api/users/{UserId}/invitation", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(UserId, service.ResentInvitationId);
    }

    [Fact]
    public async Task ResendInvitation_NonPendingAccountReturnsConflict()
    {
        FakeUserManagementService service = new() { ConflictOnResend = true };
        using AuthenticationTestFactory factory = new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", Guid.NewGuid().ToString());

        using HttpResponseMessage response = await client.PostAsync($"/api/users/{UserId}/invitation", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ResendInvitation_UnknownAccountReturnsNotFound()
    {
        FakeUserManagementService service = new() { MissingOnResend = true };
        using AuthenticationTestFactory factory = new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", Guid.NewGuid().ToString());

        using HttpResponseMessage response = await client.PostAsync($"/api/users/{UserId}/invitation", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ResendInvitation_RequiresAdministratorPolicy()
    {
        FakeUserManagementService service = new();
        using AuthenticationTestFactory factory = new(Guid.NewGuid(), service);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Operador");

        using HttpResponseMessage response = await client.PostAsync($"/api/users/{UserId}/invitation", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(service.ResentInvitationId);
    }

    internal sealed class FakeUserManagementService : IUserManagementService
    {
        private static readonly UserAccountDto User = new(
            UserId, "user@example.com", "Test User", null, "Operador", "Ativa");

        public Guid? ResentInvitationId { get; private set; }

        public bool ConflictOnResend { get; init; }

        public bool MissingOnResend { get; init; }

        public Task ResendInvitationAsync(Guid id, CancellationToken cancellationToken)
        {
            if (ConflictOnResend)
            {
                throw new AccountConflictException("Somente contas com convite pendente podem receber um novo convite.");
            }

            if (MissingOnResend)
            {
                throw new KeyNotFoundException("A conta solicitada não foi encontrada.");
            }

            ResentInvitationId = id;
            return Task.CompletedTask;
        }

        public int ListCalls { get; private set; }

        public string? Search { get; private set; }

        public string? Status { get; private set; }

        public int Page { get; private set; }

        public int PageSize { get; private set; }

        public string? Actor { get; private set; }

        public CreateUserRequest? LastCreated { get; private set; }

        public bool ConflictOnInvite { get; init; }

        public Guid? LastFoundId { get; private set; }

        public Guid? LastUpdatedId { get; private set; }

        public UpdateUserRequest? LastUpdate { get; private set; }

        public Guid? LastStatusId { get; private set; }

        public SituacaoConta? LastStatus { get; private set; }

        public Guid? ConfirmedIdentity { get; private set; }

        public string? ConfirmedEmail { get; private set; }

        public Task<PaginatedUsersDto> ListAsync(
            string? search,
            string? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            ListCalls++;
            Search = search;
            Status = status;
            Page = page;
            PageSize = pageSize;
            return Task.FromResult(new PaginatedUsersDto([], page, pageSize, 0));
        }

        public Task<UserAccountDto?> FindAsync(Guid id, CancellationToken cancellationToken)
        {
            LastFoundId = id;
            return Task.FromResult<UserAccountDto?>(User);
        }

        public Task<UserAccountDto> InviteAsync(
            CreateUserRequest request,
            string actor,
            CancellationToken cancellationToken)
        {
            if (ConflictOnInvite)
            {
                throw new AccountConflictException("O e-mail já pertence a uma conta.");
            }

            LastCreated = request;
            Actor = actor;
            return Task.FromResult(new UserAccountDto(
                Guid.NewGuid(), request.Email, request.DisplayName, null, request.Role, "ConvitePendente"));
        }

        public Task<UserAccountDto> UpdateAsync(
            Guid id,
            UpdateUserRequest request,
            string actor,
            CancellationToken cancellationToken)
        {
            LastUpdatedId = id;
            LastUpdate = request;
            return Task.FromResult(User with { DisplayName = request.DisplayName ?? User.DisplayName });
        }

        public Task SetStatusAsync(
            Guid id,
            SituacaoConta status,
            string actor,
            CancellationToken cancellationToken)
        {
            LastStatusId = id;
            LastStatus = status;
            return Task.CompletedTask;
        }

        public Task ConfirmEmailAsync(Guid identity, string confirmedEmail, CancellationToken cancellationToken)
        {
            ConfirmedIdentity = identity;
            ConfirmedEmail = confirmedEmail;
            return Task.CompletedTask;
        }
    }
}
