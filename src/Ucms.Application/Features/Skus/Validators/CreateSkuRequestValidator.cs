namespace Ucms.Application.Features.Skus.Validators;

using FluentValidation;
using Ucms.Application.Features.Skus.Commands;

public class CreateSkuRequestValidator : AbstractValidator<CreateSku.Command>
{
    public CreateSkuRequestValidator()
    {
        // SerialNumber bo'sh qoldirilsa, mahsulot asosida avtomatik generatsiya qilinadi —
        // shu sababli bu yerda NotEmpty talab qilinmaydi.
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.MeasurementUnitId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
