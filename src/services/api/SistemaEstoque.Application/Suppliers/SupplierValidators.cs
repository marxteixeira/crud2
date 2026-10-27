using FluentValidation;
using System.Net.Mail;
using SistemaEstoque.Domain.ValueObjects;

namespace SistemaEstoque.Application.Suppliers;

public sealed class CreateSupplierRequestValidator : AbstractValidator<CreateSupplierRequest>
{
    public CreateSupplierRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.TaxIdentifier).Must(IsValidTaxIdentifier)
            .WithMessage("Informe um CPF ou CNPJ válido.");
        Include(new SupplierOptionalFieldsValidator<CreateSupplierRequest>(
            request => request.Phone,
            request => request.Email,
            request => request.ContactName,
            request => request.Notes));
    }

    private static bool IsValidTaxIdentifier(string? value)
    {
        try
        {
            _ = CpfCnpj.Criar(value ?? string.Empty);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}

public sealed class UpdateSupplierRequestValidator : AbstractValidator<UpdateSupplierRequest>
{
    public UpdateSupplierRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.TaxIdentifier).Must(IsValidTaxIdentifier)
            .WithMessage("Informe um CPF ou CNPJ válido.");
        Include(new SupplierOptionalFieldsValidator<UpdateSupplierRequest>(
            request => request.Phone,
            request => request.Email,
            request => request.ContactName,
            request => request.Notes));
    }

    private static bool IsValidTaxIdentifier(string? value)
    {
        try
        {
            _ = CpfCnpj.Criar(value ?? string.Empty);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}

public sealed class SetSupplierStatusRequestValidator : AbstractValidator<SetSupplierStatusRequest>
{
    public SetSupplierStatusRequestValidator()
    {
        RuleFor(request => request.Status)
            .Must(status => status is "Active" or "Inactive")
            .WithMessage("Situação deve ser Active ou Inactive.");
    }
}

internal sealed class SupplierOptionalFieldsValidator<TRequest> : AbstractValidator<TRequest>
{
    public SupplierOptionalFieldsValidator(
        System.Linq.Expressions.Expression<Func<TRequest, string?>> phone,
        System.Linq.Expressions.Expression<Func<TRequest, string?>> email,
        System.Linq.Expressions.Expression<Func<TRequest, string?>> contactName,
        System.Linq.Expressions.Expression<Func<TRequest, string?>> notes)
    {
        RuleFor(phone).MaximumLength(30);
        RuleFor(email).MaximumLength(254);
        RuleFor(email)
            .Must(value => string.IsNullOrWhiteSpace(value) || IsValidEmail(value))
            .WithMessage("Informe um e-mail válido.");
        RuleFor(contactName).MaximumLength(200);
        RuleFor(notes).MaximumLength(2000);
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            string normalized = value.Trim();
            MailAddress address = new(normalized);
            return string.Equals(address.Address, normalized, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
