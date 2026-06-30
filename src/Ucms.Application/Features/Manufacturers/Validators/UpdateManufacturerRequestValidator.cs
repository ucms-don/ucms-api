namespace Ucms.Application.Features.Manufacturers.Validators;

using FluentValidation;
using Ucms.Application.Features.Manufacturers.Commands;

public class UpdateManufacturerRequestValidator : AbstractValidator<UpdateManufacturer.Command>
{
    public UpdateManufacturerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.NameRu).NotEmpty();
    }
}
