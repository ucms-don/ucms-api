namespace Ucms.Application.Features.Stocks.Commands;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateStock
{
    public record Command(string Name, string NameRu, string? NameEn, string? NameKa,
        string Code, StorageCondition StorageCondition, StockType StockType,
        StockCategory StockCategory, Guid OrganizationId, Guid? ParentId, Guid[] EmployeeIds);

    public sealed class Handler(IUcmsDbContext db)
    {
        public async Task<(Guid? Id, string? Error)> HandleAsync(Command cmd, CancellationToken ct)
        {
            if (await db.Stocks.AnyAsync(f => f.Code == cmd.Code, ct))
                return (null, $"'{cmd.Code}' kodi allaqachon mavjud");
            if (cmd.StockCategory == StockCategory.Central &&
                await db.Stocks.AnyAsync(f => f.StockCategory == StockCategory.Central && f.OrganizationId == cmd.OrganizationId, ct))
                return (null, "Bu tashkilot uchun markaziy ombor allaqachon mavjud");

            var stock = new Stock
            {
                Name = cmd.Name, NameRu = cmd.NameRu, NameEn = cmd.NameEn, NameKa = cmd.NameKa,
                Code = cmd.Code, StorageCondition = cmd.StorageCondition, StockType = cmd.StockType,
                StockCategory = cmd.StockCategory, ParentId = cmd.ParentId,
                OrganizationId = cmd.OrganizationId, EmployeeIds = cmd.EmployeeIds
            };
            db.Stocks.Add(stock);
            await db.SaveChangesAsync(ct);
            return (stock.Id, null);
        }
    }
}
