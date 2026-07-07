namespace Ucms.Domain.Exceptions;

/// <summary>
/// CashAccount topilmagan yoki o'chirilgan bo'lsa tashlanadi.
/// </summary>
public class CashAccountNotFoundException : AppException
{
    public CashAccountNotFoundException()
        : base("Kassa/bank hisobi topilmadi.")
    {
    }

    public CashAccountNotFoundException(string message)
        : base(message)
    {
    }

    public CashAccountNotFoundException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public CashAccountNotFoundException(Guid accountId)
        : base($"Kassa/bank hisobi topilmadi: {accountId}.")
    {
    }
}
