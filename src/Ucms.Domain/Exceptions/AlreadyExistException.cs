namespace Ucms.Domain.Exceptions;

public class AlreadyExistException : AppException
{
    private const string DEFAULT_MESSAGE = "Object with given params already exists.";
    private const string DEFAULT_MESSAGE_FORMAT = "The {0} with {1} already exists.";
    private const string DEFAULT_MESSAGE_CODE_FORMAT = "The {0} with {2} {1} already exists.";

    public AlreadyExistException()
        : this(DEFAULT_MESSAGE)
    {
    }

    public AlreadyExistException(string message)
        : base(message)
    {
    }

    public AlreadyExistException(string resourceName, object resourceKey)
        : base(DEFAULT_MESSAGE_FORMAT, resourceName, resourceKey)
    {
    }

    public AlreadyExistException(string resourceName, string propertyName, object resourceKey)
        : base(DEFAULT_MESSAGE_CODE_FORMAT, resourceName, resourceKey, propertyName)
    {
    }

    public AlreadyExistException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
