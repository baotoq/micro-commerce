namespace MicroCommerce.ApiService.Common.Messaging.Exceptions;

/// <summary>
/// Marker exception for non-retryable errors.
/// Messages that throw PermanentException skip retry and go straight to error/DLQ.
/// </summary>
public class PermanentException : Exception
{
    public PermanentException(string message) : base(message) { }

    public PermanentException(string message, Exception inner) : base(message, inner) { }
}
