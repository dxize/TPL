using System.Text;

using VirtualMachine;

namespace TestsLibrary;

public class FakeEnvironment : IEnvironment
{
    private readonly StringBuilder _output = new();
    private readonly Queue<string> _inputQueue = [];

    public string Output => _output.ToString();

    public void AddInput(string value)
    {
        _inputQueue.Enqueue(value);
    }

    public string ReadLine()
    {
        return _inputQueue.Count > 0 ? _inputQueue.Dequeue() : string.Empty;
    }

    public void Print(string text)
    {
        _output.Append(text);
    }
}