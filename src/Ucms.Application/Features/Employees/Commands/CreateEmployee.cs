namespace Ucms.Application.Features.Employees.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class CreateEmployee
{
    public record Command(string Name, string? Position, string? Phone, string? Notes, Guid? BrigadeId, Guid? UserId);

    public record Result(Guid Id, string Name);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<Result?> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = ctx.OrganizationId;
            if (!orgId.HasValue) return null;

            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            var employee = new Employee
            {
                Id             = Guid.NewGuid(),
                OrganizationId = orgId.Value,
                Name           = cmd.Name,
                Position       = cmd.Position,
                Phone          = cmd.Phone,
                Notes          = cmd.Notes,
                BrigadeId      = cmd.BrigadeId,
                UserId         = cmd.UserId,
                IsActive       = true,
                IsDeleted      = false,
                CreatedAt      = now, UpdatedAt = now,
                CreatedBy      = userId, UpdatedBy = userId,
            };

            await db.Employees.AddAsync(employee, ct);
            await db.SaveChangesAsync(ct);
            return new Result(employee.Id, employee.Name);
        }
    }
}
