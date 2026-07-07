namespace Ucms.Domain.Exceptions;

/// <summary>
/// Kassa/bank hisobida mablag' yetarli bo'lmaganda tashlanadi.
/// </summary>
public class InsufficientBalanceException : AppException
{
    public InsufficientBalanceException()
        : base("Kassada mablag' yetarli emas.")
    {
    }

    public InsufficientBalanceException(string message)
        : base(message)
    {
    }

    public InsufficientBalanceException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <param name="available">Joriy balans</param>
    /// <param name="required">Kerakli summa</param>
    public InsufficientBalanceException(decimal available, decimal required)
        : base($"Kassada mablag' yetarli emas. Mavjud: {available:N2} so'm, kerakli: {required:N2} so'm.")
    {
    }
}
