namespace Ucms.Domain.Exceptions;

public class AccessDeniedException : AppException
{
    private const string DEFAULT_MESSAGE = "Access denied";

    public AccessDeniedException()
        : this(DEFAULT_MESSAGE)
    {
    }

    public AccessDeniedException(string message)
        : base(message)
    {
    }

    public AccessDeniedException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
