namespace Ucms.Application.Features.Skus.Validators;

using FluentValidation;
using Ucms.Application.Features.Skus.Commands;

public class UpdateSkuRequestValidator : AbstractValidator<UpdateSku.Command>
{
    public UpdateSkuRequestValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.MeasurementUnitId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
