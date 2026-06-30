namespace Ucms.Application.Features.Employees.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetEmployeeById
{
    public record Query(Guid Id);

    public record EmployeeDetailDto(
        Guid Id, string Name, string? Position, string? Phone, string? Notes,
        bool IsActive, Guid? BrigadeId, string? BrigadeName,
        Guid? UserId, Guid OrganizationId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(EmployeeDetailDto? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var employee = await db.Employees
                .Where(e => e.Id == q.Id && !e.IsDeleted)
                .Select(e => new EmployeeDetailDto(
                    e.Id, e.Name, e.Position, e.Phone, e.Notes,
                    e.IsActive, e.BrigadeId, e.Brigade != null ? e.Brigade.Name : null,
                    e.UserId, e.OrganizationId, e.CreatedAt, e.UpdatedAt))
                .FirstOrDefaultAsync(ct);

            if (employee is null) return (null, false);
            if (!ctx.IsOwner && ctx.OrganizationId != employee.OrganizationId) return (null, true);
            return (employee, false);
        }
    }
}
