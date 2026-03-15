namespace Semantics.Exceptions;

public class TypeErrorException : SemanticException
{
    public TypeErrorException()
    {
    }

    public TypeErrorException(string message)
        : base(message)
    {
    }

    public TypeErrorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}