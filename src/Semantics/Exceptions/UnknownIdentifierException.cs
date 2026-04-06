namespace Semantics.Exceptions;

/// <summary>
/// Исключение из-за отсутствия идентификатора с указанным именем.
/// </summary>
public class UnknownIdentifierException : SemanticException
{
    public UnknownIdentifierException()
    {
    }

    public UnknownIdentifierException(string message)
        : base(message)
    {
    }

    public UnknownIdentifierException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}