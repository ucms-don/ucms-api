namespace Ucms.Application.Features.Customers.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class CreateCustomer
{
    public record Command(
        string Name, string? Phone, string? TaxId, string? Address, string? Notes,
        string? DirectorName = null, string? DirectorPosition = null, string? DirectorPhone = null);

    public record Result(Guid Id, string Name);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<Result?> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = ctx.OrganizationId;
            if (!orgId.HasValue) return null;

            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            var customer = new Customer
            {
                Id             = Guid.NewGuid(),
                OrganizationId = orgId.Value,
                Name           = cmd.Name,
                Phone          = cmd.Phone,
                TaxId          = cmd.TaxId,
                Address        = cmd.Address,
                Notes          = cmd.Notes,
                DirectorName     = cmd.DirectorName,
                DirectorPosition = cmd.DirectorPosition,
                DirectorPhone    = cmd.DirectorPhone,
                IsActive       = true,
                IsDeleted      = false,
                CreatedAt      = now, UpdatedAt = now,
                CreatedBy      = userId, UpdatedBy = userId,
            };

            await db.Customers.AddAsync(customer, ct);
            await db.SaveChangesAsync(ct);
            return new Result(customer.Id, customer.Name);
        }
    }
}
