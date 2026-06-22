namespace Ucms.Domain.Enums;

/// <summary>
/// CashTransaction qaysi eski (legacy) to'lov entity'sidan avtomatik yaratilganini bildiradi.
/// ADR-0001 (Option A') ga ko'ra ClientPayment/BrigadePayment/Salary/ProjectExpense
/// entity'larining o'ziga ustun qo'shilmaydi — ular CashTransaction'ga shu
/// Type-discriminator (SourceType) + nullable SourceId orqali bog'lanadi.
/// Har bir (SourceType, SourceId) juftligi uchun faqat bitta (IsDeleted=false) yozuv bo'ladi.
/// </summary>
public enum CashTransactionSourceType
{
    /// <summary>
    /// BrigadePayment.Id ga bog'langan
    /// </summary>
    BrigadePayment = 1,

    /// <summary>
    /// ClientPayment.Id ga bog'langan
    /// </summary>
    ClientPayment = 2,

    /// <summary>
    /// Salary.Id ga bog'langan
    /// </summary>
    Salary = 3,

    /// <summary>
    /// ProjectExpense.Id ga bog'langan
    /// </summary>
    ProjectExpense = 4,

    /// <summary>
    /// AccountTransfer.Id ga bog'langan — manba hisobdan chiqim (Amount + Commission)
    /// </summary>
    AccountTransferOut = 5,

    /// <summary>
    /// AccountTransfer.Id ga bog'langan — maqsad hisobga kirim (Amount)
    /// </summary>
    AccountTransferIn = 6,
}
