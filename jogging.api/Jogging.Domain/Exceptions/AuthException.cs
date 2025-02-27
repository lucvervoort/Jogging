using System.Runtime.Serialization;

namespace Jogging.Domain.Exceptions;

public class AuthException : Exception
{
    public AuthException()
    {
    }

    protected AuthException(SerializationInfo info, StreamingContext context)// : base(info, context)
    {
    }

    public AuthException(string? message) : base(message)
    {
    }

    public AuthException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}