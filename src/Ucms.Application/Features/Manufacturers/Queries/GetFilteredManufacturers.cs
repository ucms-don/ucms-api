namespace Ucms.Application.Features.Manufacturers.Queries;

using AutoMapper;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Features.Manufacturers.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class GetFilteredManufacturers
{
    public record Query(PagedRequest Filter);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<PagedResult<ManufacturerModel>> HandleAsync(Query q, CancellationToken ct)
        {
            return await db.Manufacturers.OrderBy(x => x.Name)
                .ToPagedResultAsync<Manufacturer, ManufacturerModel>(q.Filter, mapper, ct);
        }
    }
}
