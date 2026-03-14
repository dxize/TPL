namespace VirtualMachine;

public interface IEnvironment
{
    string ReadLine();

    void Print(string text);
}
