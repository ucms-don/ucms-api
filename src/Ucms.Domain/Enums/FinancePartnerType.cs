namespace Ucms.Domain.Enums;

/// <summary>
/// CashTransaction qaysi tomon (partner) bilan bog'liqligi.
/// Customer/Brigade to'lovlari hozircha ClientPayment/BrigadePayment orqali kuzatiladi —
/// bu yerda faqat hali alohida entity'si yo'q partnerlar uchun.
/// </summary>
public enum FinancePartnerType
{
    /// <summary>
    /// Yetkazib beruvchi (Supplier)
    /// </summary>
    Supplier = 1,

    /// <summary>
    /// Tashkilot egasi (Owner investitsiyasi uchun)
    /// </summary>
    Owner = 2,

    /// <summary>
    /// Kredit beruvchi tashqi tomon (bank, jismoniy/yuridik shaxs)
    /// </summary>
    Lender = 3,

    /// <summary>
    /// Boshqa
    /// </summary>
    Other = 4,
}
