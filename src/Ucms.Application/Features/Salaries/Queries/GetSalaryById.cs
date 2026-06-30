namespace Ucms.Application.Features.Salaries.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetSalaryById
{
    public record Query(Guid Id);

    public record SalaryDetailDto(
        Guid Id, Guid EmployeeId, string EmployeeName, string? Position,
        string Month, decimal Amount, string? Notes,
        Guid OrganizationId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(SalaryDetailDto? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var salary = await db.Salaries
                .Where(s => s.Id == q.Id && !s.IsDeleted)
                .Select(s => new SalaryDetailDto(
                    s.Id, s.EmployeeId, s.Employee!.Name, s.Employee.Position,
                    s.Month, s.Amount, s.Notes,
                    s.OrganizationId, s.CreatedAt, s.UpdatedAt))
                .FirstOrDefaultAsync(ct);

            if (salary is null) return (null, false);
            if (!ctx.IsOwner && ctx.OrganizationId != salary.OrganizationId) return (null, true);
            return (salary, false);
        }
    }
}
