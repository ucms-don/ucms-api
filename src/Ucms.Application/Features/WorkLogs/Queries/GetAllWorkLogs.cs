namespace Ucms.Application.Features.WorkLogs.Queries;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Abstractions;
using Ucms.Application.Persistence;
using Ucms.Domain.Enums;

public static class GetAllWorkLogs
{
    public record Query(
        Guid? ProjectId,
        Guid? BrigadeId,
        WorkLogStatus? Status,
        DateTimeOffset? From,
        DateTimeOffset? To,
        int Page,
        int Size);

    public record EstimateItemInfo(string Name, string Unit);

    public record Item(
        Guid            Id,
        Guid            ProjectId,
        string          ProjectName,
        Guid            BrigadeId,
        string          BrigadeName,
        DateTimeOffset  Date,
        decimal         Volume,
        decimal         BrigadeUnitPrice,
        decimal         TotalAmount,
        WorkLogStatus   Status,
        string?         Floor,
        string?         Zone,
        string?         Room,
        string?         Note,
        Guid?           BrigadePaymentId,
        EstimateItemInfo EstimateItem,
        DateTimeOffset  CreatedAt);

    public record Result(int Total, int Page, int Size, decimal TotalAmount, List<Item> Items);

    public sealed class Handler(IUcmsDbContext db, ICurrentContext ctx)
    {
        public async Task<(Result? Data, bool Forbidden)> HandleAsync(Query q, CancellationToken ct)
        {
            if (!ctx.IsOwner && !ctx.OrganizationId.HasValue) return (null, true);
            var locale = ctx.Locale;

            var query = db.WorkLogs
                .AsQueryable();

            if (!ctx.IsOwner && ctx.OrganizationId.HasValue)
                query = query.Where(w => w.Project!.OrganizationId == ctx.OrganizationId.Value);

            if (q.ProjectId.HasValue)  query = query.Where(w => w.ProjectId  == q.ProjectId.Value);
            if (q.BrigadeId.HasValue)  query = query.Where(w => w.BrigadeId  == q.BrigadeId.Value);
            if (q.Status.HasValue)     query = query.Where(w => w.Status     == q.Status.Value);
            if (q.From.HasValue)       query = query.Where(w => w.Date       >= q.From.Value);
            if (q.To.HasValue)         query = query.Where(w => w.Date       <= q.To.Value);

            var total       = await query.CountAsync(ct);
            var totalAmount = total > 0 ? await query.SumAsync(w => w.TotalAmount, ct) : 0;

            var items = await query
                .OrderByDescending(w => w.Date)
                .Skip((q.Page - 1) * q.Size).Take(q.Size)
                .Select(w => new Item(
                    w.Id,
                    w.ProjectId,
                    w.Project!.Name,
                    w.BrigadeId,
                    w.Brigade!.Name,
                    w.Date,
                    w.Volume,
                    w.BrigadeUnitPrice,
                    w.TotalAmount,
                    w.Status,
                    w.Floor,
                    w.Zone,
                    w.Room,
                    w.Note,
                    w.BrigadePaymentId,
                    new EstimateItemInfo(
                        locale == "ru" ? w.EstimateItem!.WorkType!.NameRu
                      : locale == "en" ? (w.EstimateItem!.WorkType!.NameEn ?? w.EstimateItem!.WorkType!.Name)
                      : locale == "ka" ? (w.EstimateItem!.WorkType!.NameKa ?? w.EstimateItem!.WorkType!.Name)
                      : w.EstimateItem!.WorkType!.Name,
                        w.EstimateItem!.MeasurementUnit!.Code),
                    w.CreatedAt))
                .ToListAsync(ct);

            return (new Result(total, q.Page, q.Size, totalAmount, items), false);
        }
    }
}
