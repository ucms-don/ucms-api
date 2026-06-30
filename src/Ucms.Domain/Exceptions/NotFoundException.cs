namespace Ucms.Domain.Exceptions;

public class NotFoundException : AppException
{
    private const string DEFAULT_MESSAGE = "Object not found";
    private const string DEFAULT_MESSAGE_FORMAT = "The {0} with {1} not found.";
    private const string DEFAULT_MESSAGE_FORMAT2 = "The {0} with {1} and {2} not found.";

    public NotFoundException()
        : this(DEFAULT_MESSAGE)
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string resourceName, object resourceKey)
        : base(DEFAULT_MESSAGE_FORMAT, resourceName, resourceKey)
    {
    }

    public NotFoundException(string resourceName, object resourceKey, object resourceKey2)
    : base(DEFAULT_MESSAGE_FORMAT2, resourceName, resourceKey, resourceKey2)
    {
    }

    public NotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
