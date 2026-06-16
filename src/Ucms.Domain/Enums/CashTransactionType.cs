namespace Ucms.Domain.Enums;

/// <summary>
/// CashTransaction operatsiya turi
/// </summary>
public enum CashTransactionType
{
    /// <summary>
    /// Yetkazib beruvchiga to'lov
    /// </summary>
    SupplierPayment = 1,

    /// <summary>
    /// Olingan kredit/qarz
    /// </summary>
    Loan = 2,

    /// <summary>
    /// Kredit/qarzni qaytarish
    /// </summary>
    LoanRepayment = 3,

    /// <summary>
    /// Tashkilot egasi investitsiyasi
    /// </summary>
    OwnerInvestment = 4,

    /// <summary>
    /// Boshqa
    /// </summary>
    Other = 5,
}
