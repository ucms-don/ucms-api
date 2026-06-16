namespace Ucms.Application.Features.Customers.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetCustomers
{
    public record Query(string? Search, bool? IsActive, int Page, int Size);

    public record Item(
        Guid Id, string Name, string? Phone, string? TaxId, string? Address,
        bool IsActive, int ProjectsCount, DateTimeOffset CreatedAt);

    public record Result(int Total, int Page, int Size, List<Item> Items);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            if (!ctx.IsOwner && !ctx.OrganizationId.HasValue) return (null, true);

            var query = db.Customers.Where(c => !c.IsDeleted);

            if (!ctx.IsOwner && ctx.OrganizationId.HasValue)
                query = query.Where(c => c.OrganizationId == ctx.OrganizationId.Value);

            if (!string.IsNullOrWhiteSpace(q.Search))
                query = query.Where(c =>
                    c.Name.Contains(q.Search) ||
                    (c.Phone != null && c.Phone.Contains(q.Search)) ||
                    (c.TaxId != null && c.TaxId.Contains(q.Search)));

            if (q.IsActive.HasValue)
                query = query.Where(c => c.IsActive == q.IsActive.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(c => c.Name)
                .Skip((q.Page - 1) * q.Size).Take(q.Size)
                .Select(c => new Item(
                    c.Id, c.Name, c.Phone, c.TaxId, c.Address,
                    c.IsActive, c.Projects.Count(p => !p.IsDeleted), c.CreatedAt))
                .ToListAsync(ct);

            return (new Result(total, q.Page, q.Size, items), false);
        }
    }
}
