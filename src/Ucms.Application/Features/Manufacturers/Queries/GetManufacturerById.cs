namespace Ucms.Application.Features.Manufacturers.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Manufacturers.DTOs;
using Ucms.Application.Persistence;

public static class GetManufacturerById
{
    public record Query(Guid Id);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<ManufacturerModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var entity = await db.Manufacturers.FirstOrDefaultAsync(f => f.Id == q.Id, ct);
            return entity is null ? null : mapper.Map<ManufacturerModel>(entity);
        }
    }
}
