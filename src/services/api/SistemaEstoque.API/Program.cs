using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SistemaEstoque.API.Configuration;
using SistemaEstoque.API.Middleware;
using SistemaEstoque.Application;
using SistemaEstoque.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
ApiOptions corsOptions = builder.Configuration.GetSection(ApiOptions.SectionName).Get<ApiOptions>()
    ?? throw new InvalidOperationException("Required configuration section 'Cors' is missing.");
if (corsOptions.AllowedOrigins.Length == 0
    || corsOptions.AllowedOrigins.Any(origin => !Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri)
        || uri.Scheme is not ("http" or "https")))
{
    throw new InvalidOperationException(
        "Configuration 'Cors:AllowedOrigins' must contain absolute HTTP or HTTPS origins.");
}

builder.Services.AddOptions<ApiOptions>()
    .Bind(builder.Configuration.GetSection(ApiOptions.SectionName))
    .Validate(
        options => options.AllowedOrigins.Length > 0
            && options.AllowedOrigins.All(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri)
                && uri.Scheme is "http" or "https"),
        "Configuration 'Cors:AllowedOrigins' must contain absolute HTTP or HTTPS origins.")
    .ValidateOnStart();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOptions.AllowedOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = async (HttpContext context, HealthReport report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(
            new { status = report.Status.ToString(), timestamp = DateTimeOffset.UtcNow },
            new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = async (HttpContext context, HealthReport report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(
            new { status = report.Status.ToString(), timestamp = DateTimeOffset.UtcNow },
            new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
});

if (app.Environment.IsEnvironment("Testing"))
{
    app.MapGet("/__test/error", ThrowTestException);
}

app.Run();

static IResult ThrowTestException()
{
    throw new InvalidOperationException("Test-only exception sentinel.");
}

public partial class Program { }
