using VirtualMachine;

using DeaInterpreter = global::Interpreter.Interpreter;

namespace Interpreter.UnitTests;

public class InterpreterTests
{
    [Fact]
    public void Executes_full_iteration2_pipeline()
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        int exitCode = interpreter.Execute(
            """
            func int main() {
                print(1, 2.5, "dea");
                return 3;
            }
            """);

        Assert.Equal(3, exitCode);
        Assert.Equal("12.5dea", environment.Output);
    }

    [Fact]
    public void Throws_for_empty_source_code()
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Action action = () => interpreter.Execute("   ");
        Assert.Throws<ArgumentException>(action);
    }

    private sealed class FakeEnvironment : IEnvironment
    {
        public string Output { get; private set; } = string.Empty;

        public string ReadLine() => string.Empty;

        public void Print(string text)
        {
            Output += text;
        }
    }
}