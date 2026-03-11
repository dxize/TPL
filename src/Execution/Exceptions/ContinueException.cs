namespace Execution.Exceptions;

public class ContinueException : Exception
{
    public ContinueException()
    {
    }

    public ContinueException(string message)
        : base(message)
    {
    }

    public ContinueException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ContinueException(double value) => ReturnValue = value;

    public double ReturnValue { get; }
}