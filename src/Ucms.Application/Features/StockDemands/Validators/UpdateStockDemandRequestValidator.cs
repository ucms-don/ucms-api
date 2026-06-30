namespace Ucms.Application.Features.StockDemands.Validators;

using FluentValidation;
using Ucms.Application.Features.StockDemands.Commands;

public class UpdateStockDemandRequestValidator : AbstractValidator<UpdateStockDemand.Command>
{
    public UpdateStockDemandRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.DemandDate).NotEmpty();
        RuleFor(x => x.SenderId).NotEmpty();
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new StockDemandItemValidator());
    }
}
