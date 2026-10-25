using FluentValidation;

namespace SistemaEstoque.Application.Locations;

public sealed class CreateRackRequestValidator : AbstractValidator<CreateRackRequest>
{
    public CreateRackRequestValidator()
    {
        RuleFor(request => request.Code).NotEmpty();
    }
}

public sealed class UpdateRackRequestValidator : AbstractValidator<UpdateRackRequest>
{
    public UpdateRackRequestValidator()
    {
        RuleFor(request => request.Code).NotEmpty();
    }
}

public sealed class CreateShelfRequestValidator : AbstractValidator<CreateShelfRequest>
{
    public CreateShelfRequestValidator()
    {
        RuleFor(request => request.Code).NotEmpty();
    }
}

public sealed class UpdateShelfRequestValidator : AbstractValidator<UpdateShelfRequest>
{
    public UpdateShelfRequestValidator()
    {
        RuleFor(request => request.Code).NotEmpty();
    }
}

public sealed class SetLocationStatusRequestValidator : AbstractValidator<SetLocationStatusRequest>
{
    public SetLocationStatusRequestValidator()
    {
        RuleFor(request => request.Status)
            .Must(status => status is "Active" or "Inactive")
            .WithMessage("Situação deve ser Active ou Inactive.");
    }
}
