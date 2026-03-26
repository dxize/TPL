using Semantics.Exceptions;

using TestsLibrary;

using DeaInterpreter = Interpreter.Interpreter;

namespace Interpreter.IntegrationTests;

public class BuiltinFunctionsTests
{
    [Theory]
    [MemberData(nameof(GetEvaluateBuiltinFunctionsData))]
    public void Can_evaluate_builtin_functions(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    public static TheoryData<string, string> GetEvaluateBuiltinFunctionsData()
    {
        return new TheoryData<string, string>
        {
            {
                """
                func int main() {
                    print(len("dea"));
                    return 0;
                }
                """,
                "3"
            },
            {
                """
                func int main() {
                    print(len(""));
                    return 0;
                }
                """,
                "0"
            },
            {
                """
                func int main() {
                    print(substr("dealang", 0, 3));
                    return 0;
                }
                """,
                "dea"
            },
            {
                """
                func int main() {
                    print(abs(-5));
                    return 0;
                }
                """,
                "5"
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidFunctionCallsData))]
    public void Throws_on_invalid_function_calls(string code, Type expectedExceptionType)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.Throws(expectedExceptionType, () => interpreter.Execute(code));
    }

    public static TheoryData<string, Type> GetInvalidFunctionCallsData()
    {
        return new TheoryData<string, Type>
        {
            {
                """
                func int main() {
                    print(len());
                    return 0;
                }
                """,
                typeof(InvalidOperationException)
            },
            {
                """
                func int main() {
                    print(len("dea", "x"));
                    return 0;
                }
                """,
                typeof(InvalidOperationException)
            },
            {
                """
                func int main() {
                    print(substr("dea"));
                    return 0;
                }
                """,
                typeof(InvalidOperationException)
            },
            {
                """
                func int main() {
                    print(abs("dea"));
                    return 0;
                }
                """,
                typeof(TypeErrorException)
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidRuntimeBuiltinCallsData))]
    public void Throws_on_invalid_runtime_builtin_calls(string code, Type expectedExceptionType)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.Throws(expectedExceptionType, () => interpreter.Execute(code));
    }

    public static TheoryData<string, Type> GetInvalidRuntimeBuiltinCallsData()
    {
        return new TheoryData<string, Type>
        {
        };
    }
}