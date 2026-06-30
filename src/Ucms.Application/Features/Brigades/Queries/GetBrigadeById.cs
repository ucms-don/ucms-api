namespace Ucms.Application.Features.Brigades.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetBrigadeById
{
    public record Query(Guid Id);

    public record BrigadeEmployeeDto(Guid Id, string Name, string? Position, string? Phone, bool IsActive);

    public record BrigadeDetailDto(
        Guid Id, string Name, string? LeaderName, string? Phone,
        bool IsActive, string Status, string? Notes,
        Guid OrganizationId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
        List<BrigadeEmployeeDto> Employees);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(BrigadeDetailDto? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var brigade = await db.Brigades
                .Where(b => b.Id == q.Id && !b.IsDeleted)
                .Select(b => new BrigadeDetailDto(
                    b.Id, b.Name, b.ForemanName, b.Phone,
                    b.IsActive, b.IsActive ? "active" : "archived", b.Notes,
                    b.OrganizationId, b.CreatedAt, b.UpdatedAt,
                    b.Employees
                        .Where(e => !e.IsDeleted)
                        .OrderBy(e => e.Name)
                        .Select(e => new BrigadeEmployeeDto(e.Id, e.Name, e.Position, e.Phone, e.IsActive))
                        .ToList()))
                .FirstOrDefaultAsync(ct);

            if (brigade is null) return (null, false);
            if (!ctx.IsOwner && ctx.OrganizationId != brigade.OrganizationId) return (null, true);
            return (brigade, false);
        }
    }
}
