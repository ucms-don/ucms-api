namespace Ucms.Application.Features.MeasurementUnits.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.MeasurementUnits.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetMeasurementUnits
{
    public record Query(MeasurementUnitType? Type = null);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<List<MeasurementUnitModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = db.MeasurementUnits.AsQueryable();
            if (q.Type.HasValue) query = query.Where(x => x.Type == q.Type);
            return mapper.Map<List<MeasurementUnitModel>>(await query.OrderBy(a => a.Name).ToListAsync(ct));
        }
    }
}
