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

    [Theory]
    [MemberData(nameof(GetInvalidFunctionCallsData))]
    public void Throws_on_invalid_function_calls(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        // Пока общий класс ошибки
        // TODO: сделать конкретный
        Assert.Throws<SemanticException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string, string> GetEvaluateBuiltinFunctionsData()
    {
        return new TheoryData<string, string>
        {
            // Длина строки
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
                    string s = "hello";
                    print(len(s));
                    return 0;
                }
                """,
                "5"
            },

            // Декомпозиция строки
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
                    string s = "dealang";
                    print(substr(s, 3, 4));
                    return 0;
                }
                """,
                "lang"
            },
            {
                """
                func int main() {
                    string s = "dea";
                    print(substr(s, 0, len(s)));
                    return 0;
                }
                """,
                "dea"
            },
        };
    }

    public static TheoryData<string> GetInvalidFunctionCallsData()
    {
        return new TheoryData<string>
        {
            // len
            {
                """
                func int main() {
                    print(len());
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(len("dea", "x"));
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(len(10));
                    return 0;
                }
                """
            },

            // substr
            {
                """
                func int main() {
                    print(substr("dea"));
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(substr("dea", "x", 1));
                    return 0;
                }
                """
            },
        };
    }
}