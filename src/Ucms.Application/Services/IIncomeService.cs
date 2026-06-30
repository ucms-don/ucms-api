namespace Ucms.Application.Services;

using Ucms.Domain.Entities;

public interface IIncomeService
{
    Task UpdateBalanceAsync(Income income, CancellationToken cancellationToken);
}
