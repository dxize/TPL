using System.Text;

using VirtualMachine;

namespace TestsLibrary;

public class FakeEnvironment : IEnvironment
{
    private readonly StringBuilder _output = new();

    public string Output => _output.ToString();

    public void Print(string text)
    {
        _output.Append(text);
    }
}