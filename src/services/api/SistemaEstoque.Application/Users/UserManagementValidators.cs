using FluentValidation;
using SistemaEstoque.Domain.Entities;

namespace SistemaEstoque.Application.Users;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(request => request.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Role).Must(IsKnownRole)
            .WithMessage("O perfil deve ser Administrador ou Operador.");
    }

    private static bool IsKnownRole(string role) =>
        Enum.TryParse(role, ignoreCase: false, out PerfilUsuario parsed)
        && Enum.IsDefined(parsed);
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(request => request.Email)
            .EmailAddress()
            .MaximumLength(320)
            .When(request => request.Email is not null);
        RuleFor(request => request.DisplayName)
            .NotEmpty()
            .MaximumLength(200)
            .When(request => request.DisplayName is not null);
        RuleFor(request => request.Role)
            .Must(role => role is null
                || (Enum.TryParse(role, ignoreCase: false, out PerfilUsuario parsed) && Enum.IsDefined(parsed)))
            .WithMessage("O perfil deve ser Administrador ou Operador.");
    }
}
