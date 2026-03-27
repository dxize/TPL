using Parser;
using TestsLibrary;

using DeaInterpreter = Interpreter.Interpreter;

namespace Interpreter.IntegrationTests;

public class ExpressionsTests
{
    [Theory]
    [MemberData(nameof(GetEvaluateExpressionsData))]
    public void Can_evaluate_expressions(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetInvalidSyntaxExpressionsData))]
    public void Rejects_invalid_syntax_expressions(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.Throws<UnexpectedLexemeException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string, string> GetEvaluateExpressionsData()
    {
        return new TheoryData<string, string>
        {
            // Арифметические операции над числами
            {
                """
                func int main() {
                    print(2 + 3);
                    return 0;
                }
                """,
                "5"
            },
            {
                """
                func int main() {
                    print(7 - 2);
                    return 0;
                }
                """,
                "5"
            },
            {
                """
                func int main() {
                    print(3 * 4);
                    return 0;
                }
                """,
                "12"
            },
            {
                """
                func int main() {
                    print(8 / 2);
                    return 0;
                }
                """,
                "4"
            },
            {
                """
                func int main() {
                    print(7 // 2);
                    return 0;
                }
                """,
                "3"
            },
            {
                """
                func int main() {
                    print(7 % 3);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(2 ^ 3);
                    return 0;
                }
                """,
                "8"
            },
            {
                """
                func int main() {
                    print(-5);
                    return 0;
                }
                """,
                "-5"
            },
            {
                """
                func int main() {
                    print(+5);
                    return 0;
                }
                """,
                "5"
            },

            // Приоритеты и скобки
            {
                """
                func int main() {
                    print(2 + 3 * 4);
                    return 0;
                }
                """,
                "14"
            },
            {
                """
                func int main() {
                    print((2 + 3) * 4);
                    return 0;
                }
                """,
                "20"
            },
            {
                """
                func int main() {
                    print((1 + (2 * 3)));
                    return 0;
                }
                """,
                "7"
            },

            // Ассоциативность
            {
                """
                func int main() {
                    print(10 - 3 - 2);
                    return 0;
                }
                """,
                "5"
            },
            {
                """
                func int main() {
                    print(16 / 4 / 2);
                    return 0;
                }
                """,
                "2"
            },
            {
                """
                func int main() {
                    print(2 ^ 3 ^ 2);
                    return 0;
                }
                """,
                "512"
            },
            {
                """
                func int main() {
                    return -2 ^ 3 ^ 2;
                }
                """,
                "-512"
            },
        };
    }

    public static TheoryData<string> GetInvalidSyntaxExpressionsData()
    {
        return new TheoryData<string>
        {
            {
                """
                func int main() {
                    print(1 +);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(* 2);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print((1 + 2);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(());
                    return 0;
                }
                """
            },
        };
    }
}