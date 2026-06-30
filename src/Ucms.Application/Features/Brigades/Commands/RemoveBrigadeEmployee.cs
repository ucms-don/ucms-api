namespace Ucms.Application.Features.Brigades.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

/// <summary>
/// Xodimni brigadadan chiqarish (BrigadeId = null)
/// </summary>
public static class RemoveBrigadeEmployee
{
    public record Command(Guid BrigadeId, Guid EmployeeId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var brigade = await db.Brigades.FindAsync([cmd.BrigadeId], ct);
            if (brigade is null || brigade.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != brigade.OrganizationId) return (false, true);

            var employee = await db.Employees.FindAsync([cmd.EmployeeId], ct);
            if (employee is null || employee.IsDeleted || employee.BrigadeId != cmd.BrigadeId)
                return (true, false);

            employee.BrigadeId = null;
            employee.UpdatedAt = DateTimeOffset.UtcNow;
            employee.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.Employees.Update(employee);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
