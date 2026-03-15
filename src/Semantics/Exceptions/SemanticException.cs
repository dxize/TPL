namespace Semantics.Exceptions;

public class SemanticException : Exception
{
    public SemanticException()
    {
    }

    public SemanticException(string message)
        : base(message)
    {
    }

    public SemanticException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}