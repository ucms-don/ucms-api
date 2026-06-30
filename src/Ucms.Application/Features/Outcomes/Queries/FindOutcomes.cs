namespace Ucms.Application.Features.Outcomes.Queries;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ucms.Application.Features.Outcomes.DTOs;
using Ucms.Application.Persistence;

public static class FindOutcomes
{
    public record Query(string Search);

    public sealed class Handler(IUcmsDbContext db, IMapper mapper)
    {
        public async Task<List<OutcomeModel>> HandleAsync(Query q, CancellationToken ct)
        {
            var s = q.Search.ToLower();
            var list = await db.Outcomes.Include(i => i.Stock)
                .Where(a => a.Name.ToLower().Contains(s) || (a.Note != null && a.Note.ToLower().Contains(s)))
                .OrderBy(a => a.Name).ToListAsync(ct);
            return mapper.Map<List<OutcomeModel>>(list);
        }
    }
}
