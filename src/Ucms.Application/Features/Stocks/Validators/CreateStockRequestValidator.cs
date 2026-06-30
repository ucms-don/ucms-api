namespace Ucms.Application.Features.Stocks.Validators;

using FluentValidation;
using Ucms.Application.Features.Stocks.Commands;

public class CreateStockRequestValidator : AbstractValidator<CreateStock.Command>
{
    public CreateStockRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.NameRu).NotEmpty();
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.OrganizationId).NotEmpty();
    }
}
