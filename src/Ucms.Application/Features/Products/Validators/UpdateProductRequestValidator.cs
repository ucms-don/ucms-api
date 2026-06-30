namespace Ucms.Application.Features.Products.Validators;

using FluentValidation;
using Ucms.Application.Features.Products.Commands;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProduct.Command>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.NameRu).NotEmpty();
    }
}
