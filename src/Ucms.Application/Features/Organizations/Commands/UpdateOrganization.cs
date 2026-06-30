namespace Ucms.Application.Features.Organizations.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;

public static class UpdateOrganization
{
    public record Command(
        Guid Id, string Name, string? TaxId,
        string? Address, string? Phone, string? Email,
        bool? IsTest = null);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(bool NotFound, bool Forbidden)> HandleAsync(Command cmd, CancellationToken ct)
        {
            var org = await db.Organizations.FindAsync([cmd.Id], ct);
            if (org is null || org.IsDeleted) return (true, false);

            var allowed = ctx.IsOwner || ctx.OrganizationId == org.Id;
            if (!allowed) return (false, true);

            org.Name      = cmd.Name;
            org.TaxId     = cmd.TaxId;
            org.Address   = cmd.Address;
            org.Phone     = cmd.Phone;
            org.Email     = cmd.Email;
            if (ctx.IsOwner && cmd.IsTest.HasValue)
                org.IsTest = cmd.IsTest.Value;
            org.UpdatedAt = DateTimeOffset.UtcNow;
            org.UpdatedBy = ctx.UserId ?? Guid.Empty;

            db.Organizations.Update(org);
            await db.SaveChangesAsync(ct);

            return (false, false);
        }
    }
}
