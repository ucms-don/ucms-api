namespace Ucms.Application.Features.Brigades.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

/// <summary>
/// Brigadaga xodimlarni biriktirish / ajratish.
/// EmployeeIds ro'yxatidagi xodimlar ushbu brigadaga biriktiriladi,
/// avval boshqa brigadada bo'lsa — ko'chiriladi.
/// </summary>
public static class AssignBrigadeEmployees
{
    public record Command(Guid BrigadeId, Guid[] EmployeeIds);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, int Assigned)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var brigade = await db.Brigades.FindAsync([cmd.BrigadeId], ct);
            if (brigade is null || brigade.IsDeleted) return (true, false, 0);
            if (!ctx.IsOwner && ctx.OrganizationId != brigade.OrganizationId) return (false, true, 0);

            var employees = await db.Employees
                .AsTracking()
                .Where(e => cmd.EmployeeIds.Contains(e.Id)
                         && e.OrganizationId == brigade.OrganizationId)
                .ToListAsync(ct);

            foreach (var emp in employees)
                emp.BrigadeId = cmd.BrigadeId;

            await db.SaveChangesAsync(ct);
            return (false, false, employees.Count);
        }
    }
}
