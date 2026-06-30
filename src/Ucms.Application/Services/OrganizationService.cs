namespace Ucms.Application.Services;

using Microsoft.AspNetCore.Identity;
using Ucms.Domain.Entities.Identity;

public class OrganizationService(UserManager<User> userManager) : IOrganizationService
{
    public async Task<string?> GetEmployeeName(Guid? employeeId, CancellationToken cancellationToken = default)
    {
        if (employeeId is null) return null;
        var user = await userManager.FindByIdAsync(employeeId.Value.ToString());
        return user?.FullName ?? user?.UserName;
    }
}
