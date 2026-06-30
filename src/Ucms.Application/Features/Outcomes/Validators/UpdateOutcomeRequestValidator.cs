namespace Ucms.Application.Features.Outcomes.Validators;

using FluentValidation;
using Ucms.Application.Features.Outcomes.Commands;
using Ucms.Application.Features.Outcomes.DTOs;
using Ucms.Domain.Enums;

public class UpdateOutcomeRequestValidator : AbstractValidator<UpdateOutcome.Command>
{
    public UpdateOutcomeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.StockId).NotEmpty();
        RuleFor(x => x.OutcomeDate).NotEmpty();
        RuleFor(x => x.OutcomeType).IsInEnum();
        RuleFor(x => x.PaymentType).IsInEnum();
        RuleFor(x => x.OutcomeStatus).IsInEnum();
        RuleFor(x => x.IncomeStockId).NotEmpty()
            .When(w => w.OutcomeType is OutcomeType.Broadcast or OutcomeType.Return);
        RuleForEach(x => x.OutcomeItems).SetValidator(new OutcomeItemValidator());
    }
}

public class OutcomeItemValidator : AbstractValidator<CreateOutcomeItemModel>
{
    public OutcomeItemValidator()
    {
        RuleFor(x => x.SkuId).NotEmpty();
        RuleFor(x => x.MeasurementUnitId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
