using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SistemaEstoque.Application.Authentication;

namespace SistemaEstoque.API.Configuration;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSupabaseAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        SupabaseOptions options = configuration.GetSection(SupabaseOptions.SectionName).Get<SupabaseOptions>()
            ?? throw new InvalidOperationException("Required configuration section 'Supabase' is missing.");
        if (!Uri.TryCreate(options.Url, UriKind.Absolute, out Uri? supabaseUri)
            || supabaseUri.Scheme is not ("http" or "https")
            || string.IsNullOrWhiteSpace(options.Audience)
            || string.IsNullOrWhiteSpace(options.AnonKey)
            || string.IsNullOrWhiteSpace(options.ServiceRoleKey)
            || (!environment.IsDevelopment() && supabaseUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "Required Supabase URL, audience, API keys, or secure transport configuration is invalid.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.Authority = options.Issuer;
                jwt.Audience = options.Audience;
                jwt.RequireHttpsMetadata = supabaseUri.Scheme == Uri.UriSchemeHttps;
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateLifetime = true,
                    NameClaimType = "sub",
                    RoleClaimType = ClaimTypes.Role
                };
                jwt.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        string? subjectClaim = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                        string? sessionClaim = context.Principal?.FindFirstValue("session_id");
                        if (!Guid.TryParse(subjectClaim, out Guid subject)
                            || !Guid.TryParse(sessionClaim, out Guid sessionId))
                        {
                            context.Fail("The access token is missing required identity claims.");
                            return;
                        }

                        ICurrentAccountAuthorization authorization =
                            context.HttpContext.RequestServices.GetRequiredService<ICurrentAccountAuthorization>();
                        if (await authorization.IsSessionRevokedAsync(sessionId, context.HttpContext.RequestAborted))
                        {
                            context.Fail("The authentication session has been revoked.");
                            return;
                        }

                        CurrentAccount? account = await authorization.FindByIdentityAsync(
                            subject,
                            context.HttpContext.RequestAborted);
                        if (account is null)
                        {
                            context.Fail("The identity is not linked to an application account.");
                            return;
                        }

                        ClaimsIdentity identity = (ClaimsIdentity)context.Principal!.Identity!;
                        foreach (Claim claim in identity.FindAll(ClaimTypes.Role).ToArray())
                        {
                            identity.RemoveClaim(claim);
                        }

                        identity.AddClaim(new Claim("account_status", account.Status));
                        identity.AddClaim(new Claim("account_id", account.Id.ToString()));
                        if (account.Status == "Ativa")
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, account.Role));
                        }
                    }
                };
            });

        services.AddApplicationAuthorization();

        return services;
    }

    public static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ActiveAccount", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("account_status", "Ativa"));
            options.AddPolicy("InvitedAccount", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("account_status", "ConvitePendente"));
            options.AddPolicy("Administrator", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("account_status", "Ativa")
                    .RequireRole("Administrador"));
        });

        return services;
    }
}
