namespace Ucms.Application.Features.CashTransactions;

using Microsoft.EntityFrameworkCore;
using Ucms.Application.Persistence;
using Ucms.Domain.Entities;
using Ucms.Domain.Enums;

/// <summary>
/// BrigadePayment/ClientPayment/Salary/ProjectExpense/Sku — barcha moliyaviy write path uchun
/// markaziy yordamchi. UpsertAsync va RemoveAsync ichida ICashBalanceService orqali
/// apply_cash_balance_delta() SP chaqiriladi (FOR UPDATE lock bilan).
///
/// MUHIM: UpsertAsync va RemoveAsync ochiq DB tranzaksiyasi ichida chaqirilishi shart —
/// SP ning FOR UPDATE lock tranzaksiya tugaganda ozod bo'ladi.
/// </summary>
public static class CashTransactionLinker
{
    /// <summary>
    /// Berilgan kassa/hisob shu tashkilotda mavjud va o'chirilmaganligini tekshiradi.
    /// </summary>
    public static Task<bool> CashAccountExistsAsync(
        IUcmsDbContext db, Guid cashAccountId, Guid organizationId, CancellationToken ct) =>
        db.CashAccounts.AnyAsync(
            a => a.Id == cashAccountId && a.OrganizationId == organizationId, ct);

    /// <summary>
    /// (SourceType, SourceId) bo'yicha bog'langan CashTransaction'ni topadi va yangilaydi,
    /// topilmasa yangi yaratadi. Har ikki holatda ham ICashBalanceService orqali balans yangilanadi.
    ///
    /// Hisob o'zgargan holat (existingAccount != newAccount):
    ///   eski hisobning deltasi teskari yo'nalishda qaytariladi, yangi hisobga to'liq delta qo'llaniladi.
    /// Hisob bir xil holat:
    ///   faqat net delta (yangi - eski) qo'llaniladi.
    /// </summary>
    public static async Task UpsertAsync(
        IUcmsDbContext db,
        ICashBalanceService balanceService,
        CashTransactionSourceType sourceType, Guid sourceId,
        Guid organizationId, Guid cashAccountId,
        CashDirection direction, CashTransactionType transactionType,
        FinancePartnerType partnerType, Guid? partnerId,
        decimal amount, DateTimeOffset date, Guid? projectId, string? note,
        Guid userId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var existing = await db.CashTransactions
            .AsTracking()
            .FirstOrDefaultAsync(
                t => t.SourceType == sourceType && t.SourceId == sourceId, ct);

        if (existing is null)
        {
            // Yangi transaksiya — to'liq delta qo'llaniladi
            await balanceService.ApplyDeltaAsync(cashAccountId, amount, direction, ct: ct);

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
            if (existing.CashAccountId == cashAccountId)
            {
                // Bir xil hisob: faqat net delta qo'llaniladi
                var oldSigned = existing.Direction == CashDirection.In
                    ?  existing.Amount
                    : -existing.Amount;
                var newSigned = direction == CashDirection.In ? amount : -amount;
                var netSigned = newSigned - oldSigned;

                if (netSigned != 0)
                {
                    var netAmount    = Math.Abs(netSigned);
                    var netDirection = netSigned > 0 ? CashDirection.In : CashDirection.Out;
                    await balanceService.ApplyDeltaAsync(cashAccountId, netAmount, netDirection, ct: ct);
                }
            }
            else
            {
                // Hisob o'zgardi: eski deltani qaytarib, yangi deltani qo'llamiz
                var reverseDir = existing.Direction == CashDirection.In
                    ? CashDirection.Out
                    : CashDirection.In;
                await balanceService.ApplyDeltaAsync(
                    existing.CashAccountId, existing.Amount, reverseDir, allowOverdraft: true, ct: ct);
                await balanceService.ApplyDeltaAsync(cashAccountId, amount, direction, ct: ct);
            }

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
        }
    }

    /// <summary>
    /// (SourceType, SourceId) bo'yicha bog'langan CashTransaction mavjud bo'lsa, soft-delete qiladi
    /// va hisobga deltani teskari yo'nalishda qaytaradi (allowOverdraft: true).
    /// </summary>
    public static async Task RemoveAsync(
        IUcmsDbContext db,
        ICashBalanceService balanceService,
        CashTransactionSourceType sourceType, Guid sourceId,
        Guid userId, CancellationToken ct)
    {
        var existing = await db.CashTransactions
            .FirstOrDefaultAsync(
                t => t.SourceType == sourceType && t.SourceId == sourceId, ct);

        if (existing is null) return;

        // Eski deltani qaytarish (overdraft ruxsat beriladi — tuzatish operatsiyasi)
        var reverseDir = existing.Direction == CashDirection.In ? CashDirection.Out : CashDirection.In;
        await balanceService.ApplyDeltaAsync(
            existing.CashAccountId, existing.Amount, reverseDir, allowOverdraft: true, ct: ct);

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
 