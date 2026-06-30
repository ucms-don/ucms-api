namespace Ucms.Application.Features.Outcomes.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Outcomes.DTOs;
using Ucms.Application.Persistence;

public static class GetOutcomeByExecutionId
{
    public record Query(Guid ExecutionId);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<OutcomeModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var outcome = await db.Outcomes
                .Include(i => i.OutcomeItems).ThenInclude(th => th.Sku!.Product)
                .Include(i => i.OutcomeItems).ThenInclude(th => th.MeasurementUnit)
                .Include(i => i.IncomeOutcome)
                .FirstOrDefaultAsync(f => f.ExecutionId == q.ExecutionId, ct);
            return outcome is null ? null : mapper.Map<OutcomeModel>(outcome);
        }
    }
}
