namespace Ucms.Application.Features.MeasurementUnits.Queries;

using AutoMapper;
using QueryForge.Abstractions;
using QueryForge.Extensions;
using QueryForge.Models;
using Ucms.Application.Features.MeasurementUnits.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class GetFilteredMeasurementUnits
{
    public record Query(string? Search, MeasurementUnitType? Type, int Page, int Size);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<PagedResult<MeasurementUnitModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = db.MeasurementUnits.AsQueryable();
            if (!string.IsNullOrEmpty(q.Search))
            {
                var s = q.Search.ToLowerInvariant().Trim();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(s) || x.NameRu.ToLower().Contains(s) ||
                    (x.NameKa != null && x.NameKa.ToLower().Contains(s)) ||
                    (x.NameEn != null && x.NameEn.ToLower().Contains(s)));
            }
            if (q.Type.HasValue) query = query.Where(x => x.Type == q.Type);
            return await query.OrderBy(c => c.Name)
                .ToPagedResultAsync<MeasurementUnit, MeasurementUnitModel>(
                    new PagedRequest { Page = q.Page, PageSize = q.Size }, mapper, ct);
        }
    }
}
