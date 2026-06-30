namespace Ucms.Application.Features.Profile.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetProfile
{
    public record Query;

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<object?> HandleAsync(Query _, CancellationToken ct)
        {
            if (ctx.UserId is null) return null;

            return await db.Users
                .Where(u => u.Id == ctx.UserId && !u.IsDeleted)
                .Select(u => (object)new
                {
                    u.Id, u.UserName, u.FullName, u.Email, u.PhoneNumber,
                    u.OrganizationId, u.CreatedAt, u.AvatarUrl,
                    Organization = u.OrganizationId == null ? null :
                        db.Organizations
                            .Where(o => o.Id == u.OrganizationId)
                            .Select(o => new { o.Id, o.Name })
                            .FirstOrDefault(),
                    Roles = db.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
                        .ToList(),
                    IsAdmin = ctx.IsAdmin,
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
