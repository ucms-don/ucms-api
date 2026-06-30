namespace Ucms.Domain.Exceptions;
using System;

public class CustomException : AppException
{
    private const string DEFAULT_MESSAGE = "A custom error occurred.";
    private const string DEFAULT_MESSAGE_FORMAT = "A custom error occurred with {0}: {1}.";
    private const string DOCUMENT_PATIENT_MISMATCH = "The document with ID '{0}' does not belong to the patient with ID '{1}'.";

    public CustomException()
        : this(DEFAULT_MESSAGE)
    {
    }

    public CustomException(string message)
        : base(message)
    {
    }

    public CustomException(string resourceName, object resourceKey)
        : base(DEFAULT_MESSAGE_FORMAT, resourceName, resourceKey)
    {
    }

    public CustomException(string resourceName, object resourceKey, object resourceKey2)
        : base(DOCUMENT_PATIENT_MISMATCH, resourceName, resourceKey, resourceKey2)
    {
    }

    public CustomException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
