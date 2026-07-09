namespace Ucms.Application.Features.Employees.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Extensions;
using Ucms.Application.Persistence;

public static class GetEmployees
{
    public record Query(bool? IsActive, Guid? BrigadeId);

    public record Item(
        Guid Id, string Name, string? Position, string? Phone, string? Notes,
        bool IsActive, Guid? BrigadeId, string? BrigadeName,
        Guid? UserId, string? UserName, string? UserEmail,
        Guid OrganizationId, DateTimeOffset CreatedAt);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<List<Item>> HandleAsync(Query q, CancellationToken ct)
        {
            var query = ctx.OrganizationId.HasValue
                ? db.Employees.IncludeChilds(ctx.OrganizationId.Value)
                : db.Employees.AsQueryable();

            if (q.IsActive.HasValue)
                query = query.Where(e => e.IsActive == q.IsActive.Value);

            if (q.BrigadeId.HasValue)
                query = query.Where(e => e.BrigadeId == q.BrigadeId.Value);

            return await query
                .OrderBy(e => e.Name)
                .Select(e => new Item(
                    e.Id, e.Name, e.Position, e.Phone, e.Notes,
                    e.IsActive, e.BrigadeId, e.Brigade != null ? e.Brigade.Name : null,
                    e.UserId,
                    e.UserId != null ? db.Users.Where(u => u.Id == e.UserId).Select(u => u.UserName).FirstOrDefault() : null,
                    e.UserId != null ? db.Users.Where(u => u.Id == e.UserId).Select(u => u.Email).FirstOrDefault() : null,
                    e.OrganizationId, e.CreatedAt))
                .ToListAsync(ct);
        }
    }
}
