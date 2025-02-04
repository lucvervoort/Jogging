﻿using System.Runtime.Serialization;

namespace Jogging.Domain.Exceptions;

public class UserException : Exception
{
    public UserException()
    {
    }

    protected UserException(SerializationInfo info, StreamingContext context)// : base(info, context)
    {
    }

    public UserException(string? message) : base(message)
    {
    }

    public UserException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}