using Reqnroll;
using TestsLibrary;
using Xunit;

using DeaInterpreter = global::Interpreter.Interpreter;

namespace Interpreter.Specs.Steps;

[Binding]
public sealed class InterpreterStepDefinitions
{
    private readonly FakeEnvironment _environment = new();
    private string _program = string.Empty;
    private int _exitCode;
    private Exception? _lastException;

    [Given(@"я загружаю программу:")]
    public void GivenILoadProgram(string program)
    {
        _program = program;
        _lastException = null;
        _exitCode = -1;
    }

    [When(@"я ввожу ""(.*)""")]
    public void WhenIInput(string input)
    {
        _environment.AddInput(input);
    }

    [When(@"я выполняю программу")]
    public void WhenIRunProgram()
    {
        DeaInterpreter interpreter = new(_environment);
        _exitCode = interpreter.Execute(_program);
    }

    [When(@"я выполняю программу с перехватом ошибки")]
    public void WhenIRunProgramWithErrorCapture()
    {
        try
        {
            DeaInterpreter interpreter = new(_environment);
            _exitCode = interpreter.Execute(_program);
        }
        catch (Exception exception)
        {
            _lastException = exception;
        }
    }

    [Then(@"код возврата равен (\d+)")]
    public void ThenExitCodeIs(int expectedExitCode)
    {
        Assert.Equal(expectedExitCode, _exitCode);
    }

    [Then(@"вывод равен ""(.*)""")]
    public void ThenOutputIs(string expectedOutput)
    {
        Assert.Equal(expectedOutput, _environment.Output);
    }

    [Then(@"произошла ошибка")]
    public void ThenErrorHappened()
    {
        Assert.NotNull(_lastException);
    }
}