namespace Ucms.Application.Features.MeasurementUnits.Validators;

using FluentValidation;
using Ucms.Application.Features.MeasurementUnits.Commands;

public class CreateMeasurementUnitRequestValidator : AbstractValidator<CreateMeasurementUnit.Command>
{
    public CreateMeasurementUnitRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.NameRu).NotEmpty();
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.Multiplier).GreaterThan(0);
    }
}
