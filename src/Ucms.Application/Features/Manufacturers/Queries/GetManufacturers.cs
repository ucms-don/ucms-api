namespace Ucms.Application.Features.Manufacturers.Queries;

using AutoMapper;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Features.Manufacturers.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class GetManufacturers
{
    public record Query(string? Search, int Page = 1, int Size = 20);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<PagedResult<ManufacturerModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = db.Manufacturers.OrderBy(x => x.Name).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.ToLower().Trim();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(s) || x.NameRu.ToLower().Contains(s) ||
                    (x.NameKa != null && x.NameKa.ToLower().Contains(s)) ||
                    (x.NameEn != null && x.NameEn.ToLower().Contains(s)));
            }
            var paged = new PagedRequest { Page = q.Page, PageSize = q.Size };
            return await query.ToPagedResultAsync<Manufacturer, ManufacturerModel>(paged, mapper, ct);
        }
    }
}
