namespace Ucms.Application.Features.Incomes.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Incomes.DTOs;
using Ucms.Application.Persistence;

public static class GetIncomeById
{
    public record Query(Guid Id);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<IncomeModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var income = await db.Incomes
                .Include(i => i.IncomeItems).ThenInclude(th => th.Sku!.MeasurementUnit)
                .Include(i => i.IncomeItems).ThenInclude(th => th.MeasurementUnit)
                .FirstOrDefaultAsync(f => f.Id == q.Id, ct);
            return income is null ? null : mapper.Map<IncomeModel>(income);
        }
    }
}
