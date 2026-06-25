namespace Ucms.Infrastructure.Services;

using Ucms.Application.Abstractions;
using Ucms.Application.Abstractions.Organization;
using Ucms.Application.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Tashqi Organization servis mavjud bo'lmaguncha ishlatiladi.
/// OrganizationId bo'yicha stock va organization ma'lumotlarini DB dan oladi.
/// </summary>
public class StubOrganizationClient(IUcmsDbContext db, ICurrentContext context) : IOrganizationClient
{
    public async Task<IEnumerable<Guid?>> GetOrganizationIds(
        Guid? organizationId = null,
        bool includeChilds = false,
        Guid? regionId = null,
        Guid? cityId = null)
    {
        var rootId = organizationId ?? context.OrganizationId;
        if (rootId is null) return [];

        // TODO: implement hierarchy traversal when Organization entity supports ParentId
        return await Task.FromResult<IEnumerable<Guid?>>(new[] { rootId });
    }

    public async Task<IEnumerable<Guid>> GetStockIds(Guid? organizationId = null)
    {
        var orgId = organizationId ?? context.OrganizationId;
        if (orgId is null) return [];

        return await db.Stocks
            .Where(s => s.OrganizationId == orgId && !s.IsDeleted)
            .Select(s => s.Id)
            .ToListAsync();
    }

    public Task<bool> CheckOrganizationBrigadeStock(Guid stockId)
    {
        // Stub: brigada integratsiyasi hali yo'q. Hech qaysi ombor brigadaga
        // biriktirilmagan deb hisoblanadi (aks holda barcha omborlarni o'chirish bloklanardi).
        return Task.FromResult(false);
    }
}
