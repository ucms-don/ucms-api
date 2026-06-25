namespace Ucms.Application.Features.Skus.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Skus.DTOs;
using Ucms.Application.Persistence;

public static class GetSkus
{
    public record Query;

    public sealed class Handler(IUcmsDbContext db, IMapper mapper, IWorkContext workContext)
    {
        public async Task<List<SkuModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var skus = await db.OrganizationSkus
                .Include(i => i.Sku!.Product)
                .Where(w => w.OrganizationId == workContext.TenantId)
                .OrderBy(a => a.Sku!.SerialNumber)
                .Select(s => s.Sku)
                .ToListAsync(ct);
            return mapper.Map<List<SkuModel>>(skus);
        }
    }
}
