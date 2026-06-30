namespace Ucms.Application.Features.Users.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetUserById
{
    public record Query(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(object? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var user = await db.Users
                .Where(u => u.Id == q.Id && !u.IsDeleted)
                .Select(u => new
                {
                    u.Id, u.UserName, u.FullName, u.Email, u.PhoneNumber,
                    u.OrganizationId, u.CreatedAt, u.UpdatedAt,
                    Roles = db.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
                        .ToList(),
                })
                .FirstOrDefaultAsync(ct);

            if (user is null) return (null, false);

            var canManage = ctx.IsOwner || (user.OrganizationId.HasValue && ctx.OrganizationId == user.OrganizationId);
            if (!canManage) return (null, true);

            return (user, false);
        }
    }
}
