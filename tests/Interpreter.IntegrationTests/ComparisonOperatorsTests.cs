using Parser;
using TestsLibrary;

using DeaInterpreter = Interpreter.Interpreter;

namespace Interpreter.IntegrationTests;

public class ComparisonOperatorsTests
{
    [Theory]
    [MemberData(nameof(GetComparisonOperatorsData))]
    public void Can_evaluate_comparison_operators(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    public static TheoryData<string, string> GetComparisonOperatorsData()
    {
        return new TheoryData<string, string>
        {
            // Меньше (<)
            {
                """
                func int main() {
                    print(5 < 10);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(10 < 5);
                    return 0;
                }
                """,
                "0"
            },
            {
                """
                func int main() {
                    print(5 < 5);
                    return 0;
                }
                """,
                "0"
            },

            // Больше (>)
            {
                """
                func int main() {
                    print(10 > 5);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(5 > 10);
                    return 0;
                }
                """,
                "0"
            },
            {
                """
                func int main() {
                    print(5 > 5);
                    return 0;
                }
                """,
                "0"
            },

            // Меньше или равно (<=)
            {
                """
                func int main() {
                    print(5 <= 10);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(5 <= 5);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(10 <= 5);
                    return 0;
                }
                """,
                "0"
            },

            // Больше или равно (>=)
            {
                """
                func int main() {
                    print(10 >= 5);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(5 >= 5);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(5 >= 10);
                    return 0;
                }
                """,
                "0"
            },

            // Равно (==)
            {
                """
                func int main() {
                    print(5 == 5);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(5 == 10);
                    return 0;
                }
                """,
                "0"
            },
            {
                """
                func int main() {
                    print(5.0 == 5);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print("dea" == "dea");
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print("dea" == "abc");
                    return 0;
                }
                """,
                "0"
            },

            // Не равно (!=)
            {
                """
                func int main() {
                    print(5 != 10);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(5 != 5);
                    return 0;
                }
                """,
                "0"
            },
            {
                """
                func int main() {
                    print("dea" != "abc");
                    return 0;
                }
                """,
                "1"
            },

            // Сравнение с переменными
            {
                """
                func int main() {
                    int x = 10;
                    int y = 5;
                    print(x > y);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    int a = 5;
                    int b = 5;
                    print(a == b);
                    return 0;
                }
                """,
                "1"
            },

            // Цепочки сравнений (слева направо)
            {
                """
                func int main() {
                    print(5 < 10 < 15);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    print(5 < 10 > 3);
                    return 0;
                }
                """,
                "0"  // (5 < 10) = 1, затем 1 > 3 = 0
            },

            // Сравнение с арифметикой
            {
                """
                func int main() {
                    print(5 + 3 > 2 * 4);
                    return 0;
                }
                """,
                "0"
            },
            {
                """
                func int main() {
                    print(10 - 5 == 10 / 2);
                    return 0;
                }
                """,
                "1"
            },
        };
    }
}
