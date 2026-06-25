namespace Ucms.Application.Features.StockDemands.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.StockDemands.DTOs;
using Ucms.Application.Persistence;
using Ucms.Application.Services;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class UpdateStockDemand
{
    public record Command(Guid Id, string Name, string? Note, DateTimeOffset DemandDate,
        StockDemandStatus DemandStatus, Guid SenderId, Guid RecipientId, IEnumerable<StockDemandItemModel> Items);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext, IOrganizationService organizationService)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var demand = await db.StockDemands.AsTracking()
                .Include(i => i.StockDemandItems)
                .FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);

            if (demand is null || demand.DemandStatus != StockDemandStatus.Draft)
                return (true, null);

            demand.Name = cmd.Name; demand.Note = cmd.Note; demand.DemandStatus = cmd.DemandStatus;
            demand.DemandDate = cmd.DemandDate; demand.SenderId = cmd.SenderId; demand.RecipientId = cmd.RecipientId;
            demand.EmployeeId = workContext.EmployeeId;
            demand.EmployeeName = await organizationService.GetEmployeeName(workContext.EmployeeId, ct);
            demand.StockDemandItems.Clear();
            foreach (var item in cmd.Items)
                demand.StockDemandItems.Add(new StockDemandItem
                {
                    Amount = item.Amount, ProductId = item.ProductId,
                    MeasurementUnitId = item.MeasurementUnitId, NotApproved = item.NotApproved, Note = item.Note
                });

            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
