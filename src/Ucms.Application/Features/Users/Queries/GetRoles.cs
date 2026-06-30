namespace Ucms.Application.Features.Users.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;

public static class GetRoles
{
    public record Query;

    public record Item(Guid Id, string? Name, string? Description);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<List<Item>> HandleAsync(Query _, CancellationToken ct)
        {
            return await db.Roles
                .OrderBy(r => r.Name)
                .Select(r => new Item(r.Id, r.Name, r.Description))
                .ToListAsync(ct);
        }
    }
}
