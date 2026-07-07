namespace Ucms.Application.Features.Projects.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetProjects
{
    public record Query(ProjectStatus? Status, string? StatusString, int Page, int Size);

    public record Item(
        Guid Id, string Name, string? Address, string? ContractNumber,
        decimal? ContractValue, decimal EstimatesTotal, ProjectStatus Status, string StatusString,
        DateTimeOffset? StartDate, DateTimeOffset? EndDate,
        Guid OrganizationId, DateTimeOffset CreatedAt,
        Guid? CustomerId, string? CustomerName);

    public record Result(int Total, int Page, int Size, List<Item> Items);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var orgId = ctx.IsOwner ? (Guid?)null : ctx.OrganizationId;
            if (orgId is null && !ctx.IsOwner) return (null, true);

            var query = db.Projects;
            if (orgId.HasValue) query = query.Where(p => p.OrganizationId == orgId.Value);

            // Status filtri — enum yoki UI string orqali
            if (q.Status.HasValue)
            {
                query = query.Where(p => p.Status == q.Status.Value);
            }
            else if (!string.IsNullOrEmpty(q.StatusString))
            {
                var mapped = MapStatusStringToEnum(q.StatusString);
                if (mapped.HasValue)
                    query = query.Where(p => p.Status == mapped.Value);
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((q.Page - 1) * q.Size).Take(q.Size)
                .Select(p => new Item(
                    p.Id, p.Name, p.Address, p.ContractNumber,
                    p.ContractValue,
                    p.Estimates.SelectMany(a => a.Sections).SelectMany(s => s.EstimateItems)
                        .Sum(i => (decimal?)Math.Round(i.Volume * i.ClientUnitPrice, 2)) ?? 0m,
                    p.Status, MapStatusToString(p.Status),
                    p.StartDate, p.EndDate, p.OrganizationId, p.CreatedAt,
                    p.CustomerId, p.Customer != null ? p.Customer.Name : null))
                .ToListAsync(ct);

            return (new Result(total, q.Page, q.Size, items), false);
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

        private static ProjectStatus? MapStatusStringToEnum(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "planning"  => ProjectStatus.Planning,
                "active"    => ProjectStatus.InProgress,
                "completed" => ProjectStatus.Completed,
                "suspended" => ProjectStatus.Suspended,
                "archived"  => ProjectStatus.Cancelled,
                _           => null,
            };
        }
    }
}
