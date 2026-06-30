namespace Ucms.Application.Features.StockDemands.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Features.StockDemands.DTOs;
using Ucms.Application.Persistence;
using Ucms.Application.Services;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateStockDemand
{
    public record Command(string Name, string? Note, DateTimeOffset DemandDate, StockDemandStatus DemandStatus,
        Guid SenderId, Guid RecipientId, IEnumerable<StockDemandItemModel> Items);

    public sealed class Handler(IUcmsDbContext db, IWorkContext workContext, IOrganizationService organizationService)
    {
        public async Task<(Guid? Id, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (await db.StockDemands.AnyAsync(f => f.Name == cmd.Name, ct))
                return (null, $"'{cmd.Name}' nomi allaqachon mavjud");

            var demand = new StockDemand
            {
                Name = cmd.Name, Note = cmd.Note, DemandStatus = cmd.DemandStatus,
                DemandDate = cmd.DemandDate, SenderId = cmd.SenderId, RecipientId = cmd.RecipientId,
                EmployeeId = workContext.EmployeeId,
                EmployeeName = await organizationService.GetEmployeeName(workContext.EmployeeId, ct)
            };
            foreach (var item in cmd.Items)
                demand.StockDemandItems.Add(new StockDemandItem
                {
                    Amount = item.Amount, ProductId = item.ProductId,
                    MeasurementUnitId = item.MeasurementUnitId, NotApproved = item.NotApproved, Note = item.Note
                });

            db.StockDemands.Add(demand);
            await db.SaveChangesAsync(ct);
            return (demand.Id, null);
        }
    }
}
