using FluentValidation;

namespace SistemaEstoque.Application.Products;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Price)
            .Must(IsValidPrice)
            .WithMessage("O preço deve ser maior ou igual a zero e ter no máximo duas casas decimais.");
    }

    private static bool IsValidPrice(decimal? price) =>
        price is null || (price >= 0 && decimal.Round(price.Value, 2) == price.Value);
}

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().When(request => request.Name is not null);
        RuleFor(request => request.Quantity).GreaterThanOrEqualTo(0).When(request => request.Quantity.HasValue);
        RuleFor(request => request.Price)
            .Must(price => price is null || (price >= 0 && decimal.Round(price.Value, 2) == price.Value))
            .WithMessage("O preço deve ser maior ou igual a zero e ter no máximo duas casas decimais.");
    }
}

public sealed class SetProductStatusRequestValidator : AbstractValidator<SetProductStatusRequest>
{
    public SetProductStatusRequestValidator()
    {
        RuleFor(request => request.Status)
            .Must(status => status is "Active" or "Inactive")
            .WithMessage("Situação deve ser Active ou Inactive.");
    }
}
