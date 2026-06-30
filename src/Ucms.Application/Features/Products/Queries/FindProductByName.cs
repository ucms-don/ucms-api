namespace Ucms.Application.Features.Products.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Products.DTOs;
using Ucms.Application.Persistence;

public static class FindProductByName
{
    public record Query(string Name);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<ProductModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var s = q.Name.ToLower();
            var product = await db.Products.FirstOrDefaultAsync(f =>
                f.Name.ToLower().Contains(s) ||
                (f.NameKa != null && f.NameKa.ToLower().Contains(s)) ||
                f.NameRu.ToLower().Contains(s) ||
                (f.NameEn != null && f.NameEn.ToLower().Contains(s)), ct);
            return product is null ? null : mapper.Map<ProductModel>(product);
        }
    }
}
