namespace Ucms.Application.Features.Dashboard.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.Dashboard.DTOs;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetDashboard
{
    public record Query;

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<DashboardDto> HandleAsync(Query _, CancellationToken ct)
        {
            var orgId = ctx.OrganizationId;

            var projectsQuery = db.Projects.Where(p => !p.IsDeleted);
            if (orgId.HasValue) projectsQuery = projectsQuery.Where(p => p.OrganizationId == orgId.Value);

            var ps = await projectsQuery
                .GroupBy(_ => true)
                .Select(g => new
                {
                    Total      = g.Count(),
                    Planning   = g.Count(p => p.Status == ProjectStatus.Planning),
                    InProgress = g.Count(p => p.Status == ProjectStatus.InProgress),
                    Completed  = g.Count(p => p.Status == ProjectStatus.Completed),
                    Suspended  = g.Count(p => p.Status == ProjectStatus.Suspended),
                })
                .FirstOrDefaultAsync(ct);

            var projectStats = new DashboardProjectStatsDto(
                ps?.Total      ?? 0,
                ps?.Planning   ?? 0,
                ps?.InProgress ?? 0,
                ps?.Completed  ?? 0,
                ps?.Suspended  ?? 0);

            var brigadesQuery = db.Brigades.Where(b => !b.IsDeleted);
            if (orgId.HasValue) brigadesQuery = brigadesQuery.Where(b => b.OrganizationId == orgId.Value);

            var bs = await brigadesQuery
                .GroupBy(_ => true)
                .Select(g => new { Total = g.Count(), Active = g.Count(b => b.IsActive) })
                .FirstOrDefaultAsync(ct);

            var brigadeStats = new DashboardBrigadeStatsDto(
                bs?.Total  ?? 0,
                bs?.Active ?? 0);

            var activeProjectIds = await projectsQuery
                .Where(p => p.Status == ProjectStatus.InProgress)
                .Select(p => p.Id)
                .ToListAsync(ct);

            decimal clientReceived = 0, brigadePaid = 0, workedTotal = 0;

            if (activeProjectIds.Count != 0)
            {
                clientReceived = await db.ClientPayments
                    .Where(p => activeProjectIds.Contains(p.ProjectId))
                    .SumAsync(p => p.Amount, ct);

                brigadePaid = await db.BrigadePayments
                    .Where(p => activeProjectIds.Contains(p.ProjectId))
                    .SumAsync(p => p.Amount, ct);

                workedTotal = await db.WorkLogs
                    .Where(w => activeProjectIds.Contains(w.ProjectId))
                    .SumAsync(w => w.TotalAmount, ct);
            }

            var finance = new DashboardFinanceDto(
                ClientReceived: clientReceived,
                BrigadePaid:    brigadePaid,
                WorkedTotal:    workedTotal,
                BrigadeDebt:    workedTotal - brigadePaid);

            var recentWorkLogs = await db.WorkLogs
                .Where(w => !orgId.HasValue || activeProjectIds.Contains(w.ProjectId))
                .OrderByDescending(w => w.CreatedAt)
                .Take(5)
                .Select(w => new DashboardWorkLogDto(
                    w.Id,
                    w.Date,
                    w.TotalAmount,
                    w.Status,
                    w.Project!.Name,
                    w.Brigade!.Name,
                    w.EstimateItem!.WorkType!.Name))
                .ToListAsync(ct);

            var recentPayments = await db.ClientPayments
                .Where(p => !orgId.HasValue || activeProjectIds.Contains(p.ProjectId))
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new DashboardPaymentDto(
                    p.Id,
                    p.Date,
                    p.Amount,
                    p.PaymentMethod,
                    p.Project!.Name,
                    p.Act != null ? p.Act.ActNumber : null))
                .ToListAsync(ct);

            return new DashboardDto(
                Projects:       projectStats,
                Brigades:       brigadeStats,
                Finance:        finance,
                RecentWorkLogs: recentWorkLogs,
                RecentPayments: recentPayments);
        }
    }
}
