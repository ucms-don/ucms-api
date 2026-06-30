namespace Ucms.Application.Features.Manufacturers.Validators;

using FluentValidation;
using Ucms.Application.Features.Manufacturers.Commands;

public class CreateManufacturerRequestValidator : AbstractValidator<CreateManufacturer.Command>
{
    public CreateManufacturerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.NameRu).NotEmpty();
    }
}
