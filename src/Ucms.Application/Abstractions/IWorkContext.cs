namespace Ucms.Application.Abstractions;

/// <summary>
/// ICurrentContext dan meros — orqaga moslik uchun saqlanmoqda
/// </summary>
public interface IWorkContext : ICurrentContext
{
    /// <summary>
    /// Tenant ID (ICurrentContext.OrganizationId bilan bir xil)
    /// </summary>
    Guid? TenantId { get; }

    Guid? EmployeeId { get; }
}
