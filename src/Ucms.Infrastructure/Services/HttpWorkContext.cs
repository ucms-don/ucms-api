namespace Ucms.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using Ucms.Application.Abstractions;

/// <summary>
/// IWorkContext implementation — ICurrentContext ustiga TenantId va EmployeeId qo'shadi.
/// TenantId = OrganizationId (JWT organization_id claim)
/// EmployeeId = UserId (JWT sub claim)
/// </summary>
public class HttpWorkContext(IHttpContextAccessor httpContextAccessor)
    : HttpCurrentContext(httpContextAccessor), IWorkContext
{
    public Guid? TenantId => OrganizationId;
    public Guid? EmployeeId => UserId;
}
