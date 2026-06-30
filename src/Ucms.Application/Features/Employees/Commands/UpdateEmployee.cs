namespace Ucms.Application.Features.Employees.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class UpdateEmployee
{
    public record Command(Guid Id, string Name, string? Position, string? Phone, string? Notes, Guid? BrigadeId, Guid? UserId, bool IsActive);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var employee = await db.Employees.FindAsync([cmd.Id], ct);
            if (employee is null || employee.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != employee.OrganizationId) return (false, true);

            employee.Name      = cmd.Name;
            employee.Position  = cmd.Position;
            employee.Phone     = cmd.Phone;
            employee.Notes     = cmd.Notes;
            employee.BrigadeId = cmd.BrigadeId;
            employee.UserId    = cmd.UserId;
            employee.IsActive  = cmd.IsActive;
            employee.UpdatedAt = DateTimeOffset.UtcNow;
            employee.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.Employees.Update(employee);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
