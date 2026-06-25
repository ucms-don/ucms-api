namespace Ucms.Application.Features.Skus.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Skus.DTOs;
using Ucms.Application.Persistence;

public static class FindSkus
{
    public record Query(string Search);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<List<SkuModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var s = q.Search.ToLower();
            var skus = await db.Skus
                .Include(a => a.Product)
                .Where(a =>
                    a.Product!.Name.ToLower().Contains(s) || a.Product!.NameEn!.ToLower().Contains(s) ||
                    a.Product!.NameRu.ToLower().Contains(s) || a.Product!.NameKa!.ToLower().Contains(s) ||
                    a.SerialNumber.Contains(s))
                .OrderBy(a => a.SerialNumber)
                .ToListAsync(ct);
            return mapper.Map<List<SkuModel>>(skus);
        }
    }
}
