namespace Ucms.Application.Features.Organizations.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetOrganizations
{
    public record Query;

    public record Item(
        Guid Id, string Name, string? TaxId, string? Address,
        string? Phone, string? Email, OrganizationType Type,
        bool IsTest, DateTimeOffset CreatedAt);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<List<Item>> HandleAsync(Query _, CancellationToken ct)
        {
            var query = db.Organizations
                .AsQueryable();

            if (!ctx.IsOwner && ctx.OrganizationId.HasValue)
                query = query.Where(o => o.Id == ctx.OrganizationId.Value);

            return await query
                .OrderBy(o => o.Name)
                .Select(o => new Item(o.Id, o.Name, o.TaxId, o.Address,
                    o.Phone, o.Email, o.Type, o.IsTest, o.CreatedAt))
                .ToListAsync(ct);
        }
    }
}
