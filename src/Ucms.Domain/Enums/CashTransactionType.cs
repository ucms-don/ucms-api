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

    /// <summary>
    /// Brigada to'lovi (BrigadePayment bilan bog'langan kassa harakati)
    /// </summary>
    BrigadePayment = 6,

    /// <summary>
    /// Mijoz/buyurtmachi to'lovi (ClientPayment bilan bog'langan kassa harakati)
    /// </summary>
    ClientPayment = 7,

    /// <summary>
    /// Maosh to'lovi (Salary bilan bog'langan kassa harakati)
    /// </summary>
    SalaryPayment = 8,

    /// <summary>
    /// Loyiha xarajati (ProjectExpense bilan bog'langan kassa harakati)
    /// </summary>
    ProjectExpense = 9,

    /// <summary>
    /// Kassadan kassaga o'tkazma (AccountTransfer bilan bog'langan kassa harakati)
    /// </summary>
    AccountTransfer = 10,
}
