namespace Ucms.Application.Features.WorkTypes.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.WorkTypes.DTOs;
using Ucms.Application.Persistence;

public static class GetWorkTypes
{
    public record Query;

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<List<WorkTypeModel>> HandleAsync(Query q, CancellationToken ct)
        {
            return mapper.Map<List<WorkTypeModel>>(
                await db.WorkTypes.OrderBy(a => a.Name).ToListAsync(ct));
        }
    }
}
