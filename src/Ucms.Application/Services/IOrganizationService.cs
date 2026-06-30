namespace Ucms.Application.Services;

public interface IOrganizationService
{
    Task<string?> GetEmployeeName(Guid? employeeId, CancellationToken cancellationToken = default);
}
