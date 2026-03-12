namespace Execution.Exceptions;

public class ReturnException : Exception
{
    public ReturnException()
    {
    }

    public ReturnException(string message)
        : base(message)
    {
    }

    public ReturnException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ReturnException(object value) => ReturnValue = value;

    public object ReturnValue { get; }
}