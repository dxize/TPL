namespace Interpreter;

using VirtualMachine;

public sealed class ConsoleEnvironment : IEnvironment
{
    /// <summary>
    /// Печатает строку (без перевода строки).
    /// </summary>
    public void Print(string text)
    {
        Console.Write(text);
    }
}