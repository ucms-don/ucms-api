namespace Ucms.Application.Features.Skus.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Skus.DTOs;
using Ucms.Application.Persistence;

public static class GetProductSkus
{
    public record Query(Guid ProductId);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext)
    {
        public async Task<List<SkuModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var skus = await db.OrganizationSkus
                .Include(i => i.Sku!.Product)
                .Include(i => i.Sku!.MeasurementUnit)
                .Where(w => w.OrganizationId == workContext.TenantId && w.Sku!.ProductId == q.ProductId)
                .Select(w => w.Sku!)
                .OrderBy(a => a.SerialNumber)
                .ToListAsync(ct);
            return mapper.Map<List<SkuModel>>(skus);
        }
    }
}
