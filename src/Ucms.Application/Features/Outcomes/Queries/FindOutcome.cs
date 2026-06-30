namespace Ucms.Application.Features.Outcomes.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Outcomes.DTOs;
using Ucms.Application.Persistence;

public static class FindOutcome
{
    public record Query(string Name);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<OutcomeModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var outcome = await db.Outcomes
                .Include(a => a.OutcomeItems).ThenInclude(a => a.Sku!.Product)
                .Include(i => i.OutcomeItems).ThenInclude(th => th.MeasurementUnit)
                .Include(i => i.IncomeOutcome)
                .FirstOrDefaultAsync(f => f.Name == q.Name, ct);
            return outcome is null ? null : mapper.Map<OutcomeModel>(outcome);
        }
    }
}
