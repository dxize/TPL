namespace Semantics.Exceptions;

/// <summary>
/// Исключение из-за некорректного вызова функции.
/// </summary>
public class InvalidFunctionCallException : SemanticException
{
    public InvalidFunctionCallException()
    {
    }

    public InvalidFunctionCallException(string message)
        : base(message)
    {
    }

    public InvalidFunctionCallException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}