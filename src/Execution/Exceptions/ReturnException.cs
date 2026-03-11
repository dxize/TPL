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

    public ReturnException(double value) => ReturnValue = value;

    public double ReturnValue { get; }
}