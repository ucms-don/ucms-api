namespace Ucms.Application.Features.Stocks.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class UpdateStock
{
    public record Command(Guid Id, string Name, string NameRu, string? NameEn, string? NameKa,
        string? Code, StorageCondition StorageCondition, StockType StockType,
        StockCategory StockCategory, Guid OrganizationId, Guid? ParentId, Guid[] EmployeeIds);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(bool NotFound, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var stock = await db.Stocks.AsTracking().FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
            if (stock is null) return (true, null);
            if (cmd.Code is not null && await db.Stocks.AnyAsync(a => a.Id != cmd.Id && a.Code == cmd.Code, ct))
                return (false, $"'{cmd.Code}' kodi allaqachon mavjud");
            if (cmd.StockCategory == StockCategory.Central &&
                await db.Stocks.AnyAsync(a => a.StockCategory == StockCategory.Central && a.Id != cmd.Id && a.OrganizationId == cmd.OrganizationId, ct))
                return (false, "Bu tashkilot uchun markaziy ombor allaqachon mavjud");

            stock.Name = cmd.Name; stock.NameRu = cmd.NameRu; stock.NameEn = cmd.NameEn; stock.NameKa = cmd.NameKa;
            stock.StorageCondition = cmd.StorageCondition; stock.StockType = cmd.StockType;
            stock.StockCategory = cmd.StockCategory; stock.ParentId = cmd.ParentId;
            stock.OrganizationId = cmd.OrganizationId; stock.EmployeeIds = cmd.EmployeeIds;
            await db.SaveChangesAsync(ct);
            return (false, null);
        }
    }
}
