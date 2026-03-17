using TestsLibrary;

using DeaInterpreter = global::Interpreter.Interpreter;

namespace Interpreter.UnitTests;

public class InterpreterTests
{
    [Fact]
    public void Executes_full_iteration2_pipeline()
    {
        string code = Samples.GetSampleProgram("example.dea");
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        int exitCode = interpreter.Execute(code);

        Assert.Equal(0, exitCode);
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
}