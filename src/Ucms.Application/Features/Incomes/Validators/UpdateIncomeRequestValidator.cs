namespace Ucms.Application.Features.Incomes.Validators;

using FluentValidation;
using Ucms.Application.Features.Incomes.Commands;

public class UpdateIncomeRequestValidator : AbstractValidator<UpdateIncome.Command>
{
    public UpdateIncomeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.StockId).NotEmpty();
        RuleFor(x => x.IncomeDate).NotEmpty();
        RuleFor(x => x.IncomeType).IsInEnum();
        RuleFor(x => x.PaymentType).IsInEnum();
        RuleFor(x => x.IncomeStatus).IsInEnum();
        RuleForEach(x => x.IncomeItems).SetValidator(new IncomeItemValidator());
    }
}
