namespace Execution.Exceptions;

public class BreakException : Exception
{
    public BreakException()
    {
    }

    public BreakException(string message)
        : base(message)
    {
    }

    public BreakException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public BreakException(double value) => ReturnValue = value;

    public double ReturnValue { get; }
}