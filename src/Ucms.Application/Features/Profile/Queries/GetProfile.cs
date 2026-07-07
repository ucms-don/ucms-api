namespace Ucms.Application.Features.Profile.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Constants;

public static class GetProfile
{
    public record Query;

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<object?> HandleAsync(Query _, CancellationToken ct)
        {
            if (ctx.UserId is null) return null;

            var user = await db.Users
                .Where(u => u.Id == ctx.UserId)
                .Select(u => new
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
                    RoleIds = db.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Select(ur => ur.RoleId)
                        .ToList(),
                })
                .FirstOrDefaultAsync(ct);

            if (user is null) return null;

            // Foydalanuvchi rollaridan permission claimlarini yuklash
            var permissions = await db.RoleClaims
                .Where(rc => user.RoleIds.Contains(rc.RoleId) && rc.ClaimType == Permissions.ClaimType)
                .Select(rc => rc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            return new
            {
                user.Id, user.UserName, user.FullName, user.Email, user.PhoneNumber,
                user.OrganizationId, user.CreatedAt, user.AvatarUrl,
                user.Organization,
                user.Roles,
                Permissions = permissions,
                ctx.IsAdmin,
            };
        }
    }
}
