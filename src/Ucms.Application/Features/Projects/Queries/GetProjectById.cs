namespace Ucms.Application.Features.Projects.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Projects.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetProjectById
{
    public record Query(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(ProjectDetailDto? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var project = await db.Projects
                .Where(p => p.Id == q.Id && !p.IsDeleted)
                .Select(p => new ProjectDetailDto(
                    p.Id,
                    p.Name,
                    p.ClientName,
                    p.CustomerId,
                    p.Customer != null ? p.Customer.Name : null,
                    p.Address,
                    p.Description,
                    p.ContractNumber,
                    p.ContractDate,
                    p.ContractValue,
                    p.StartDate,
                    p.EndDate,
                    p.Status,
                    MapStatusToString(p.Status),
                    p.OrganizationId,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.Estimates.SelectMany(a => a.Sections.OrderBy(s => s.Order).Select(s => new ProjectSectionDto(
                        s.Id,
                        s.Name,
                        s.Order,
                        s.EstimateItems.OrderBy(i => i.Order).Select(i => new ProjectEstimateItemDto(
                            i.Id,
                            i.WorkType!.Name,
                            i.MeasurementUnit!.Code,
                            i.Volume,
                            i.ClientUnitPrice,
                            i.BrigadeUnitPrice,
                            i.Volume * i.ClientUnitPrice,
                            i.Volume * i.BrigadeUnitPrice,
                            i.Order)))))))
                .FirstOrDefaultAsync(ct);

            if (project is null) return (null, false);
            if (!ctx.IsOwner && ctx.OrganizationId != project.OrganizationId) return (null, true);
            return (project, false);
        }

        private static string MapStatusToString(ProjectStatus status)
        {
            return status switch
            {
                ProjectStatus.Planning   => "planning",
                ProjectStatus.InProgress => "active",
                ProjectStatus.Completed  => "completed",
                ProjectStatus.Suspended  => "suspended",
                ProjectStatus.Cancelled  => "archived",
                _                        => "planning",
            };
        }
    }
}
