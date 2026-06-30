namespace Ucms.Application.Features.Stocks.Validators;

using FluentValidation;
using Ucms.Application.Features.Stocks.Commands;

public class UpdateStockRequestValidator : AbstractValidator<UpdateStock.Command>
{
    public UpdateStockRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.NameRu).NotEmpty();
        RuleFor(x => x.OrganizationId).NotEmpty();
    }
}
