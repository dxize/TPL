namespace Interpreter;

using VirtualMachine;

public sealed class ConsoleEnvironment : IEnvironment
{
    public string ReadLine()
    {
        return Console.ReadLine() ?? string.Empty;
    }

    /// <summary>
    /// Печатает строку (без перевода строки).
    /// </summary>
    public void Print(string text)
    {
        Console.Write(text);
    }
}