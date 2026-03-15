namespace Semantics.Exceptions;

public class InvalidExpressionException : SemanticException
{
    public InvalidExpressionException()
    {
    }

    public InvalidExpressionException(string message)
        : base(message)
    {
    }

    public InvalidExpressionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}