namespace Ucms.Application.Features.Employees.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class DeleteEmployee
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var employee = await db.Employees.FindAsync([cmd.Id], ct);
            if (employee is null || employee.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != employee.OrganizationId) return (false, true);

            employee.IsDeleted = true;
            employee.UpdatedAt = DateTimeOffset.UtcNow;
            employee.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.Employees.Update(employee);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
