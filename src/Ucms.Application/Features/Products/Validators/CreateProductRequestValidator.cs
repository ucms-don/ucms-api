namespace Ucms.Application.Features.Products.Validators;

using FluentValidation;
using Ucms.Application.Features.Products.Commands;

public class CreateProductRequestValidator : AbstractValidator<CreateProduct.Command>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.NameRu).NotEmpty();
    }
}
