namespace Ucms.Application.Features.Products.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Products.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetProducts
{
    public record Query(string? Search, List<ProductType>? Types, int Page = 1, int Size = 20);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<PagedResult<ProductModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = db.Products.OrderBy(x => x.Name).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.ToLowerInvariant().Trim();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(s) ||
                    x.NameRu.ToLower().Contains(s) ||
                    (x.NameKa != null && x.NameKa.ToLower().Contains(s)) ||
                    (x.NameEn != null && x.NameEn.ToLower().Contains(s)) ||
                    (x.Code != null && x.Code.ToLower().Contains(s)));
            }

            if (q.Types != null && q.Types.Count > 0)
                query = query.Where(x => q.Types.Contains(x.Type));

            var paged = new PagedRequest { Page = q.Page, PageSize = q.Size };
            return await query.ToPagedResultAsync<Domain.Entities.Product, ProductModel>(paged, mapper, ct);
        }
    }
}
