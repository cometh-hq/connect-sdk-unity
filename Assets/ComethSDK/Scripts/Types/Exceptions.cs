using System;

public class SignerNotFoundException : Exception
{
    public SignerNotFoundException(string message) : base(message) { }
}