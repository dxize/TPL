namespace Semantics.Exceptions;

/// <summary>
/// Исключение из-за некорректного присваивания.
/// </summary>
public class InvalidAssignmentException : SemanticException
{
    public InvalidAssignmentException()
    {
    }

    public InvalidAssignmentException(string message)
        : base(message)
    {
    }

    public InvalidAssignmentException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}