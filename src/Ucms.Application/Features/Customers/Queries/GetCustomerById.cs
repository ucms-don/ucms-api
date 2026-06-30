namespace Ucms.Application.Features.Customers.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class GetCustomerById
{
    public record Query(Guid Id);

    public record ProjectSummary(Guid Id, string Name, string? ContractNumber, decimal? ContractValue);

    public record CustomerDetailDto(
        Guid Id, string Name, string? Phone, string? TaxId, string? Address, string? Notes,
        bool IsActive, Guid OrganizationId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
        List<ProjectSummary> Projects,
        string? DirectorName, string? DirectorPosition, string? DirectorPhone);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(CustomerDetailDto? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            var customer = await db.Customers
                .Where(c => c.Id == q.Id && !c.IsDeleted)
                .Select(c => new CustomerDetailDto(
                    c.Id, c.Name, c.Phone, c.TaxId, c.Address, c.Notes,
                    c.IsActive, c.OrganizationId, c.CreatedAt, c.UpdatedAt,
                    c.Projects.Where(p => !p.IsDeleted)
                        .Select(p => new ProjectSummary(p.Id, p.Name, p.ContractNumber, p.ContractValue))
                        .ToList(),
                    c.DirectorName, c.DirectorPosition, c.DirectorPhone))
                .FirstOrDefaultAsync(ct);

            if (customer is null) return (null, false);
            if (!ctx.IsOwner && ctx.OrganizationId != customer.OrganizationId) return (null, true);
            return (customer, false);
        }
    }
}
