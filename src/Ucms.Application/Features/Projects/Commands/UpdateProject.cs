namespace Ucms.Application.Features.Projects.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateProject
{
    public record Command(
        Guid Id, string Name, string? ClientName, string? Address, string? Description,
        string? ContractNumber, DateTimeOffset? ContractDate,
        DateTimeOffset? StartDate, DateTimeOffset? EndDate,
        decimal? ContractValue, ProjectStatus Status, Guid? CustomerId);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden, bool CustomerNotFound)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var project = await db.Projects.FindAsync([cmd.Id], ct);
            if (project is null || project.IsDeleted) return (true, false, false);
            if (!ctx.IsOwner && ctx.OrganizationId != project.OrganizationId) return (false, true, false);

            if (cmd.CustomerId.HasValue)
            {
                var customerExists = await db.Customers
                    .AnyAsync(c => c.Id == cmd.CustomerId.Value && !c.IsDeleted && c.OrganizationId == project.OrganizationId, ct);
                if (!customerExists) return (false, false, true);
            }

            project.Name           = cmd.Name;
            project.ClientName     = cmd.ClientName;
            project.CustomerId     = cmd.CustomerId;
            project.Address        = cmd.Address;
            project.Description    = cmd.Description;
            project.ContractNumber = cmd.ContractNumber;
            project.ContractDate   = cmd.ContractDate;
            project.ContractValue  = cmd.ContractValue;
            project.StartDate      = cmd.StartDate;
            project.EndDate        = cmd.EndDate;
            project.Status         = cmd.Status;
            project.UpdatedAt      = DateTimeOffset.UtcNow;
            project.UpdatedBy      = ctx.UserId ?? Guid.Empty;

            db.Projects.Update(project);
            await db.SaveChangesAsync(ct);
            return (false, false, false);
        }
    }
}
