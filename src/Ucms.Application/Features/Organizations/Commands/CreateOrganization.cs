namespace Ucms.Application.Features.Organizations.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateOrganization
{
    public record Command(
        string Name, string? TaxId, string? Address,
        string? Phone, string? Email,
        OrganizationType Type = OrganizationType.Tenant,
        bool IsTest = false);

    public record Result(Guid Id, string Name);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<Result> HandleAsync(Command cmd, CancellationToken ct)
        {
            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            var org = new Organization
            {
                Id        = Guid.NewGuid(),
                Name      = cmd.Name,
                TaxId     = cmd.TaxId,
                Address   = cmd.Address,
                Phone     = cmd.Phone,
                Email     = cmd.Email,
                Type      = cmd.Type,
                IsTest    = ctx.IsOwner && cmd.IsTest,
                IsDeleted = false,
                CreatedAt = now, UpdatedAt = now,
                CreatedBy = userId, UpdatedBy = userId,
            };

            await db.Organizations.AddAsync(org, ct);
            await db.SaveChangesAsync(ct);

            return new Result(org.Id, org.Name);
        }
    }
}
