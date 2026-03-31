namespace Semantics.Exceptions;

/// <summary>
/// Исключение из-за повторного объявления идентификатора с тем же именем.
/// </summary>
public class DuplicateIdentifierException : SemanticException
{
    public DuplicateIdentifierException()
    {
    }

    public DuplicateIdentifierException(string message)
        : base(message)
    {
    }

    public DuplicateIdentifierException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
