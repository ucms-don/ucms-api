namespace Ucms.Application.Features.Manufacturers.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Manufacturers.DTOs;
using Ucms.Application.Persistence;

public static class FindManufacturerByName
{
    public record Query(string Name);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<ManufacturerModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var s = q.Name.ToLower();
            var entity = await db.Manufacturers.FirstOrDefaultAsync(f =>
                f.Name.ToLower().Contains(s) || f.NameRu.ToLower().Contains(s) ||
                (f.NameKa != null && f.NameKa.ToLower().Contains(s)) ||
                (f.NameEn != null && f.NameEn.ToLower().Contains(s)), ct);
            return entity is null ? null : mapper.Map<ManufacturerModel>(entity);
        }
    }
}
