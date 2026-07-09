namespace Ucms.Application.Features.Brigades.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Extensions;
using Ucms.Application.Persistence;

public static class GetBrigades
{
    public record Query(bool? IsActive, string? StatusString);

    public record Item(
        Guid Id, string Name, string? LeaderName, string? Phone,
        bool IsActive, string Status, string? Notes,
        Guid OrganizationId, DateTimeOffset CreatedAt);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<List<Item>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = ctx.OrganizationId.HasValue
                ? db.Brigades.IncludeChilds(ctx.OrganizationId.Value)
                : db.Brigades.AsQueryable();

            // IsActive filtri — bool yoki UI string "active"/"archived"
            if (q.IsActive.HasValue)
            {
                query = query.Where(b => b.IsActive == q.IsActive.Value);
            }
            else if (!string.IsNullOrEmpty(q.StatusString))
            {
                var active = q.StatusString.ToLowerInvariant() == "active";
                query = query.Where(b => b.IsActive == active);
            }

            return await query
                .OrderBy(b => b.Name)
                .Select(b => new Item(
                    b.Id, b.Name, b.ForemanName, b.Phone,
                    b.IsActive, b.IsActive ? "active" : "archived", b.Notes,
                    b.OrganizationId, b.CreatedAt))
                .ToListAsync(ct);
        }
    }
}
