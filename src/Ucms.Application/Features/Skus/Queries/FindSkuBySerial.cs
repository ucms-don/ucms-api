namespace Ucms.Application.Features.Skus.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Skus.DTOs;
using Ucms.Application.Persistence;

public static class FindSkuBySerial
{
    public record Query(string SerialNumber);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<SkuModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var sku = await db.Skus.Include(s => s.Product).FirstOrDefaultAsync(f => f.SerialNumber == q.SerialNumber, ct);
            return sku is null ? null : mapper.Map<SkuModel>(sku);
        }
    }
}
