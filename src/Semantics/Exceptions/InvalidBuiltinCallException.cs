namespace Semantics.Exceptions;

/// <summary>
/// Исключение из-за некорректного вызова встроенной функции.
/// </summary>
public class InvalidBuiltinCallException : SemanticException
{
    public InvalidBuiltinCallException()
    {
    }

    public InvalidBuiltinCallException(string message)
        : base(message)
    {
    }

    public InvalidBuiltinCallException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}