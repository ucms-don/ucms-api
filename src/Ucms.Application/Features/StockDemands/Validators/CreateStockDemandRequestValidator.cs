namespace Ucms.Application.Features.StockDemands.Validators;

using FluentValidation;
using Ucms.Application.Features.StockDemands.Commands;
using Ucms.Application.Features.StockDemands.DTOs;

public class CreateStockDemandRequestValidator : AbstractValidator<CreateStockDemand.Command>
{
    public CreateStockDemandRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.DemandDate).NotEmpty();
        RuleFor(x => x.SenderId).NotEmpty();
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new StockDemandItemValidator());
    }
}

public class StockDemandItemValidator : AbstractValidator<StockDemandItemModel>
{
    public StockDemandItemValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.MeasurementUnitId).NotEmpty();
    }
}
