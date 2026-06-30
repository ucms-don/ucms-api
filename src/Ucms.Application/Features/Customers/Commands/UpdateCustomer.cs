namespace Ucms.Application.Features.Customers.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class UpdateCustomer
{
    public record Command(
        Guid Id, string Name, string? Phone, string? TaxId, string? Address, string? Notes, bool IsActive,
        string? DirectorName = null, string? DirectorPosition = null, string? DirectorPhone = null);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var customer = await db.Customers.FindAsync([cmd.Id], ct);
            if (customer is null || customer.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != customer.OrganizationId) return (false, true);

            customer.Name      = cmd.Name;
            customer.Phone     = cmd.Phone;
            customer.TaxId     = cmd.TaxId;
            customer.Address   = cmd.Address;
            customer.Notes     = cmd.Notes;
            customer.DirectorName     = cmd.DirectorName;
            customer.DirectorPosition = cmd.DirectorPosition;
            customer.DirectorPhone    = cmd.DirectorPhone;
            customer.IsActive  = cmd.IsActive;
            customer.UpdatedAt = DateTimeOffset.UtcNow;
            customer.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.Customers.Update(customer);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
