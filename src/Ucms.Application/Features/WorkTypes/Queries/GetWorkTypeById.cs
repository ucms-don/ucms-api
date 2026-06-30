namespace Ucms.Application.Features.WorkTypes.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.WorkTypes.DTOs;
using Ucms.Application.Persistence;

public static class GetWorkTypeById
{
    public record Query(Guid Id);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<WorkTypeModel?> HandleAsync(Query q, CancellationToken ct)
        {
            var entity = await db.WorkTypes.FirstOrDefaultAsync(f => f.Id == q.Id, ct);
            return entity is null ? null : mapper.Map<WorkTypeModel>(entity);
        }
    }
}
