namespace Ucms.Application.Features.CashAccounts.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

public static class CreateCashAccount
{
    public record Command(string Name, CashAccountType Type, string? Notes);

    public record Result(Guid Id, string Name);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<Result?> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = ctx.IsOwner ? ctx.OrganizationId : (Guid?)null;
            if (!orgId.HasValue) return null;

            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            var account = new CashAccount
            {
                Id             = Guid.NewGuid(),
                OrganizationId = orgId.Value,
                Name           = cmd.Name,
                Type           = cmd.Type,
                Notes          = cmd.Notes,
                IsActive       = true,
                IsDeleted      = false,
                CreatedAt      = now, UpdatedAt = now,
                CreatedBy      = userId, UpdatedBy = userId,
            };

            await db.CashAccounts.AddAsync(account, ct);
            await db.SaveChangesAsync(ct);
            return new Result(account.Id, account.Name);
        }
    }
}
