namespace Semantics.Exceptions;

/// <summary>
/// Исключение, возникающее во время выполнения программы.
/// </summary>
public class RuntimeException : SemanticException
{
    public RuntimeException()
    {
    }

    public RuntimeException(string message)
        : base(message)
    {
    }

    public RuntimeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}