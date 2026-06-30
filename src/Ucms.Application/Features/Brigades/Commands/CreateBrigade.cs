namespace Ucms.Application.Features.Brigades.Commands;

using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;

public static class CreateBrigade
{
    public record Command(string Name, string? ForemanName, string? Phone, string? Notes);

    public record Result(Guid Id, string Name);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<Result?> HandleAsync(Command cmd, CancellationToken ct)
        {
            var orgId = ctx.OrganizationId;
            if (!orgId.HasValue) return null;

            var now    = DateTimeOffset.UtcNow;
            var userId = ctx.UserId ?? Guid.Empty;

            var brigade = new Brigade
            {
                Id             = Guid.NewGuid(),
                OrganizationId = orgId.Value,
                Name           = cmd.Name,
                ForemanName    = cmd.ForemanName,
                Phone          = cmd.Phone,
                Notes          = cmd.Notes,
                IsActive       = true,
                IsDeleted      = false,
                CreatedAt      = now, UpdatedAt = now,
                CreatedBy      = userId, UpdatedBy = userId,
            };

            await db.Brigades.AddAsync(brigade, ct);
            await db.SaveChangesAsync(ct);
            return new Result(brigade.Id, brigade.Name);
        }
    }
}
