namespace Semantics.Exceptions;

/// <summary>
/// Исключение, возникающее во время выполнения программы.
/// </summary>
public class RuntimeExceptionException : SemanticException
{
    public RuntimeExceptionException()
    {
    }

    public RuntimeExceptionException(string message)
        : base(message)
    {
    }

    public RuntimeExceptionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
