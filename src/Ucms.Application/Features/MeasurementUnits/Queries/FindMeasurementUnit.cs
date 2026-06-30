namespace Ucms.Application.Features.MeasurementUnits.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.MeasurementUnits.DTOs;
using Ucms.Application.Persistence;

public static class FindMeasurementUnit
{
    public record Query(string Code);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<MeasurementUnitModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var entity = await db.MeasurementUnits.FirstOrDefaultAsync(f => f.Code == q.Code, ct);
            return entity is null ? null : mapper.Map<MeasurementUnitModel>(entity);
        }
    }
}
