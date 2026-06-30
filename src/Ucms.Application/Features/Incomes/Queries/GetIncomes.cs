namespace Ucms.Application.Features.Incomes.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Incomes.DTOs;
using Ucms.Application.Persistence;

public static class GetIncomes
{
    public record Query;

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext)
    {
        public async Task<List<IncomeModel>> HandleAsync(Query q, CancellationToken ct)
        {
            return await db.Incomes.Include(i => i.IncomeItems).Include(i => i.Stock)
                .Where(w => w.Stock!.OrganizationId == workContext.TenantId)
                .OrderByDescending(a => a.IncomeDate)
                .ProjectTo<IncomeModel>(mapper.ConfigurationProvider)
                .ToListAsync(ct);
        }
    }
}
