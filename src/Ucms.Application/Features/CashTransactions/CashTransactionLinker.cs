namespace Ucms.Application.Features.CashTransactions;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

/// <summary>
/// BrigadePayment/ClientPayment/Salary/ProjectExpense — eski (legacy) to'lov entity'larini
/// CashAccount/CashTransaction tizimiga bog'lash uchun umumiy yordamchi (ADR-0001, Option A').
///
/// Eski entity'larning o'ziga hech qanday ustun (CashAccountId va h.k.) qo'shilmaydi.
/// Buning o'rniga CashTransaction'ning o'zida bitta umumiy Type-discriminator
/// (<see cref="CashTransactionSourceType"/> SourceType) + nullable SourceId ishlatiladi —
/// shu orqali har qanday eski yozuv (manba) o'ziga mos bitta CashTransaction bilan
/// 1:1 bog'lanadi. Bu "hammasiga alohida-alohida ustun qo'shish" o'rniga bitta umumiy
/// mexanizm orqali barcha 4 entity uchun ishlaydi.
/// </summary>
public static class CashTransactionLinker
{
    /// <summary>
    /// Berilgan kassa/hisob shu tashkilotda mavjud va o'chirilmaganligini tekshiradi.
    /// </summary>
    public static Task<bool> CashAccountExistsAsync(
        IUcmsDbContext db, Guid cashAccountId, Guid organizationId, CancellationToken ct) =>
        db.CashAccounts.AnyAsync(a => a.Id == cashAccountId && !a.IsDeleted && a.OrganizationId == organizationId, ct);

    /// <summary>
    /// (SourceType, SourceId) bo'yicha bog'langan CashTransaction'ni topadi, topilsa yangilaydi,
    /// topilmasa yangi yaratadi. SaveChangesAsync chaqirilmaydi — bu chaqiruvchi handler'ning
    /// o'z SaveChangesAsync'i bilan bitta tranzaksiyada saqlanishi uchun.
    /// </summary>
    public static async Task UpsertAsync(
        IUcmsDbContext db,
        CashTransactionSourceType sourceType, Guid sourceId,
        Guid organizationId, Guid cashAccountId,
        CashDirection direction, CashTransactionType transactionType,
        FinancePartnerType partnerType, Guid? partnerId,
        decimal amount, DateTimeOffset date, Guid? projectId, string? note,
        Guid userId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var existing = await db.CashTransactions
            .FirstOrDefaultAsync(t => t.SourceType == sourceType && t.SourceId == sourceId && !t.IsDeleted, ct);

        if (existing is null)
        {
            await db.CashTransactions.AddAsync(new CashTransaction
            {
                Id              = Guid.NewGuid(),
                OrganizationId  = organizationId,
                CashAccountId   = cashAccountId,
                Direction       = direction,
                TransactionType = transactionType,
                PartnerType     = partnerType,
                PartnerId       = partnerId,
                Amount          = amount,
                Date            = date,
                ProjectId       = projectId,
                Note            = note,
                SourceType      = sourceType,
                SourceId        = sourceId,
                IsDeleted       = false,
                CreatedAt       = now, UpdatedAt = now,
                CreatedBy       = userId, UpdatedBy = userId,
            }, ct);
        }
        else
        {
            existing.CashAccountId   = cashAccountId;
            existing.Direction       = direction;
            existing.TransactionType = transactionType;
            existing.PartnerType     = partnerType;
            existing.PartnerId       = partnerId;
            existing.Amount          = amount;
            existing.Date            = date;
            existing.ProjectId       = projectId;
            existing.Note            = note;
            existing.UpdatedAt       = now;
            existing.UpdatedBy       = userId;
            db.CashTransactions.Update(existing);
        }
    }

    /// <summary>
    /// (SourceType, SourceId) bo'yicha bog'langan CashTransaction mavjud bo'lsa, soft-delete qiladi.
    /// Manba (legacy yozuv) o'chirilganda yoki undan CashAccountId olib tashlanganda chaqiriladi.
    /// </summary>
    public static async Task RemoveAsync(
        IUcmsDbContext db, CashTransactionSourceType sourceType, Guid sourceId, Guid userId, CancellationToken ct)
    {
        var existing = await db.CashTransactions
            .FirstOrDefaultAsync(t => t.SourceType == sourceType && t.SourceId == sourceId && !t.IsDeleted, ct);

        if (existing is null) return;

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.UpdatedBy = userId;
        db.CashTransactions.Update(existing);
    }
}
