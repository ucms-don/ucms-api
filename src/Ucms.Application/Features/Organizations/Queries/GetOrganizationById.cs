namespace Ucms.Application.Features.Organizations.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetOrganizationById
{
    public record Query(Guid Id);

    public record Result(
        Guid Id, string Name, string? TaxId, string? Address,
        string? Phone, string? Email, OrganizationType Type, bool IsTest,
        DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
        int ProjectCount, int BrigadeCount);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var org = await db.Organizations
                .Where(o => o.Id == q.Id && !o.IsDeleted)
                .Select(o => new Result(
                    o.Id, o.Name, o.TaxId, o.Address, o.Phone, o.Email,
                    o.Type, o.IsTest, o.CreatedAt, o.UpdatedAt,
                    o.Projects.Count(p => !p.IsDeleted),
                    o.Brigades.Count(b => !b.IsDeleted)))
                .FirstOrDefaultAsync(ct);

            if (org is null) return (null, false);

            var allowed = ctx.IsOwner || ctx.OrganizationId == org.Id;
            return allowed ? (org, false) : (null, true);
        }
    }
}
