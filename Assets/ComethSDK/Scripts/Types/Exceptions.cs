using System;

public class SignerException : Exception
{
    public SignerException(string message) : base(message) { }
}

public class SignerNotFoundException : SignerException
{
    public SignerNotFoundException(string message) : base(message) { }
}

public class SignerUnauthorizedException : SignerException
{
    public SignerUnauthorizedException(string message) : base(message) { }
}