namespace Ucms.Application.Features.StockDemands.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.StockDemands.DTOs;
using Ucms.Application.Persistence;

public static class FindStockDemand
{
    public record Query(string Name);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<StockDemandModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var entity = await db.StockDemands
                .Include(i => i.StockDemandItems)
                .FirstOrDefaultAsync(f => f.Name == q.Name, ct);
            return entity is null ? null : mapper.Map<StockDemandModel>(entity);
        }
    }
}
