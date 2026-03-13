namespace Semantics.Exceptions;

public sealed class TypeErrorException : SemanticException
{
    public TypeErrorException(string message)
        : base(message)
    {
    }
}