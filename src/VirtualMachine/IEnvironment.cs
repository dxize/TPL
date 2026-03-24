namespace VirtualMachine;

public interface IEnvironment
{
    string ReadLine();

    /// <summary>
    /// Печатает строку (без перевода строки).
    /// </summary>
    void Print(string text);
}