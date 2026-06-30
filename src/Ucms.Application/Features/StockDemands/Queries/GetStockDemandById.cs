namespace Ucms.Application.Features.StockDemands.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.StockDemands.DTOs;
using Ucms.Application.Persistence;

public static class GetStockDemandById
{
    public record Query(Guid Id);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<StockDemandModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var entity = await db.StockDemands
                .Include(i => i.StockDemandItems).ThenInclude(th => th.Product)
                .Include(i => i.StockDemandItems).ThenInclude(th => th.MeasurementUnit)
                .FirstOrDefaultAsync(f => f.Id == q.Id, ct);
            return entity is null ? null : mapper.Map<StockDemandModel>(entity);
        }
    }
}
