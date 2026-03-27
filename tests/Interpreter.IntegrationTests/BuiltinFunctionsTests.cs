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
    [MemberData(nameof(GetInvalidBuiltinFunctionCallsData))]
    public void Rejects_invalid_builtin_function_calls(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        // TODO переделать на конкретную ошибку
        Assert.Throws<SemanticException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string, string> GetEvaluateBuiltinFunctionsData()
    {
        return new TheoryData<string, string>
        {
            // Built-in функции (строки)
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

            // Built-in функции (числа)
            {
                """
                func int main() {
                    print(abs(-5));
                    return 0;
                }
                """,
                "5"
            },
            {
                """
                func int main() {
                    print(abs(-3.14));
                    return 0;
                }
                """,
                "3.14"
            },
            {
                """
                func int main() {
                    print(min(3, 1));
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(min(5, 2, 7, 1));
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(min(3.5, 1.2));
                    return 0;
                }
                """,
                "1.2"
            },
            {
                """
                func int main() {
                    print(max(3, 1));
                    return 0;
                }
                """,
                "3"
            },
            {
                """
                func int main() {
                    print(max(5, 2, 7, 1));
                    return 0;
                }
                """,
                "7"
            },
            {
                """
                func int main() {
                    print(max(3.5, 1.2));
                    return 0;
                }
                """,
                "3.5"
            },
        };
    }

    public static TheoryData<string> GetInvalidBuiltinFunctionCallsData()
    {
        return new TheoryData<string>
        {
            // Ошибки (Built-in операции над строками, семантика)
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
            {
                """
                func int main() {
                    print(substr("dea", 0, "x"));
                    return 0;
                }
                """
            },

            // Ошибки (Built-in функции чисел, семантика)
            {
                """
                func int main() {
                    print(abs());
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(abs(1, 2));
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(min());
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(min(1));
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(max());
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(max(1));
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(abs("dea"));
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(min(1, 1.5));
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(max(1.5, 1));
                    return 0;
                }
                """
            },
        };
    }
}