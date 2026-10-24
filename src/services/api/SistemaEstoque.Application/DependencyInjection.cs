using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SistemaEstoque.Application.Authentication;
using SistemaEstoque.Application.Users;

namespace SistemaEstoque.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<GetCurrentAccount>();
        services.AddScoped<ActivateInvitedAccount>();
        services.AddScoped<RevokeCurrentSession>();
        services.AddScoped<ConfirmEmailChange>();
        return services;
    }
}
