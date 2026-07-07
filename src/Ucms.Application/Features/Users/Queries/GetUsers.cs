namespace Ucms.Application.Features.Users.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetUsers
{
    public record Query(
        Guid? OrganizationId, string? Search, bool? IsActive,
        int Page, int Size);

    public record Result(int Total, int Page, int Size, List<object> Items);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var targetOrgId = ctx.IsOwner ? q.OrganizationId : ctx.OrganizationId;

            var query = db.Users
                .AsQueryable();

            if (targetOrgId.HasValue)
                query = query.Where(u => u.OrganizationId == targetOrgId.Value);
            else if (!ctx.IsOwner)
                return (null, true);

            if (!string.IsNullOrWhiteSpace(q.Search))
                query = query.Where(u =>
                    u.UserName!.Contains(q.Search) ||
                    (u.FullName != null && u.FullName.Contains(q.Search)) ||
                    u.Email!.Contains(q.Search));

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(u => u.FullName ?? u.UserName)
                .Skip((q.Page - 1) * q.Size).Take(q.Size)
                .Select(u => (object)new
                {
                    u.Id, u.UserName, u.FullName, u.Email, u.PhoneNumber,
                    u.OrganizationId, u.CreatedAt,
                    Roles = db.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
                        .ToList(),
                })
                .ToListAsync(ct);

            return (new Result(total, q.Page, q.Size, items), false);
        }
    }
}
