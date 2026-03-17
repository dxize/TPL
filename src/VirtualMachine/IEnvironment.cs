namespace VirtualMachine;

public interface IEnvironment
{
    /// <summary>
    /// Печатает строку (без перевода строки).
    /// </summary>
    void Print(string text);
}