namespace Ucms.Application.Features.Customers.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class DeleteCustomer
{
    public record Command(Guid Id);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var customer = await db.Customers.FindAsync([cmd.Id], ct);
            if (customer is null || customer.IsDeleted) return (true, false);
            if (!ctx.IsOwner && ctx.OrganizationId != customer.OrganizationId) return (false, true);

            customer.IsDeleted = true;
            customer.UpdatedAt = DateTimeOffset.UtcNow;
            customer.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.Customers.Update(customer);
            await db.SaveChangesAsync(ct);
            return (false, false);
        }
    }
}
