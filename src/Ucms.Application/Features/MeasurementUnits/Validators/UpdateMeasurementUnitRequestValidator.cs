namespace Ucms.Application.Features.MeasurementUnits.Validators;

using FluentValidation;
using Ucms.Application.Features.MeasurementUnits.Commands;

public class UpdateMeasurementUnitRequestValidator : AbstractValidator<UpdateMeasurementUnit.Command>
{
    public UpdateMeasurementUnitRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.NameRu).NotEmpty();
        RuleFor(x => x.Multiplier).GreaterThan(0);
    }
}
