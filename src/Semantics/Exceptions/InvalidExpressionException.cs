namespace Semantics.Exceptions;

public sealed class InvalidExpressionException : SemanticException
{
    public InvalidExpressionException(string message)
        : base(message)
    {
    }
}