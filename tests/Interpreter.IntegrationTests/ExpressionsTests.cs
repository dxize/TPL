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
                    print(-2 ^ 3 ^ 2);
                    return 0;
                }
                """,
                "-512"
            },

            // Операции над строками
            {
                """
                func int main() {
                    print("dea" + "lang");
                    return 0;
                }
                """,
                "dealang"
            },
            {
                """
                func int main() {
                    string s = "dea"; print(s + "lang");
                    return 0;
                }
                """,
                "dealang"
            },

            // Операторы сравнения
            {
                """
                func int main() {
                    print(5 > 3);
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func int main() {
                    print(5.5 >= 3);
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func int main() {
                    print(5.0 == 5);
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func int main() {
                    print(5 == 5.0);
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func int main() {
                    print(2.5 <= 2.5);
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func int main() {
                    print(2.5 < 2.5);
                    return 0;
                }
                """,
                "false"
            },
            {
                """
                func int main() {
                    print("apple" == "apple");
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func int main() {
                    print("apple" != "apple");
                    return 0;
                }
                """,
                "false"
            },
            {
                """
                func int main() {
                    print(true != false);
                    return 0;
                }
                """,
                "true"
            },

            // Логические операции и булевы литералы
            {
                """
                func int main() {
                    print(true && false);
                    return 0;
                }
                """,
                "false"
            },
            {
                """
                func int main() {
                    print(1 && 0);
                    return 0;
                }
                """,
                "false"
            },
            {
                """
                func int main() {
                    print("abc" && "");
                    return 0;
                }
                """,
                "false"
            },
            {
                """
                func int main() {
                    print(true || false);
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func int main() {
                    print(1 || 0);
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func int main() {
                    print("abc" || "");
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func int main() {
                    print(!true);
                    return 0;
                }
                """,
                "false"
            },
            {
                """
                func int main() {
                    print(!!false);
                    return 0;
                }
                """,
                "false"
            },
            {
                """
                func int main() {
                    print(!10);
                    return 0;
                }
                """,
                "false"
            },
            {
                """
                func int main() {
                    print(!"");
                    return 0;
                }
                """,
                "true"
            },

            // short-circuit (вычисления по короткой схеме)
            {
                """
                func int main() {
                    print(false && (1 / 0 == 0));
                    return 0;
                }
                """,
                "false"
            },
            {
                """
                func int main() {
                    print(true || (1 / 0 == 0));
                    return 0;
                }
                """,
                "true"
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