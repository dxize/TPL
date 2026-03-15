namespace Interpreter;

using VirtualMachine;

public sealed class ConsoleEnvironment : IEnvironment
{
    public string ReadLine()
    {
        return Console.ReadLine() ?? string.Empty;
    }

    public void Print(string text)
    {
        Console.Write(text);
    }
}