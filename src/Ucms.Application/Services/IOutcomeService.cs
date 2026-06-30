namespace Ucms.Application.Services;

using Ucms.Application.Features.Outcomes.DTOs;
using Ucms.Domain.Entities;

public interface IOutcomeService
{
    public Task UpdateBalanceAsync(Outcome outcome, CancellationToken cancellationToken);
    public Task UpdateIncomeOutcome(Guid incomeId, CancellationToken cancellationToken);
    public Task CancelIncomeOutcome(Guid incomeId, CancellationToken cancellationToken);
    public IncomeOutcome CreateIncomeOutcome(Outcome outcome, Guid incomeStockId);
    public Income CreateIncome(Outcome outcome, Guid incomeStockId);
    public IEnumerable<OrganizationSku> CreateOrganizationSkus(Outcome outcome, Guid incomeStockId);
    public Task ValidateOutcomeItems(IEnumerable<CreateOutcomeItemModel> outcomeItems, Guid stockId, CancellationToken cancellationToken);
    public Task ValidateOutcomeItems(IEnumerable<OutcomeItem> outcomeItems, Guid stockId, CancellationToken cancellationToken);
}
