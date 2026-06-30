namespace Ucms.Application.Abstractions.Organization;

public interface IOrganizationClient
{
    public Task<IEnumerable<Guid?>> GetOrganizationIds(Guid? organizationId = null, bool includeChilds = false, Guid? regionId = null, Guid? cityId = null);
    public Task<IEnumerable<Guid>> GetStockIds(Guid? organizationId = null);
    public Task<bool> CheckOrganizationBrigadeStock(Guid stockId);
}
