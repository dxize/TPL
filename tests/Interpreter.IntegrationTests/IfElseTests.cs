using Parser;
using Semantics.Exceptions;
using TestsLibrary;

using DeaInterpreter = Interpreter.Interpreter;

namespace Interpreter.IntegrationTests;

public class IfElseTests
{
    [Theory]
    [MemberData(nameof(GetControlFlowData))]
    public void Can_evaluate_control_flow(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetInvalidControlFlowSyntaxData))]
    public void Rejects_invalid_control_flow_syntax(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.Throws<UnexpectedLexemeException>(() => interpreter.Execute(code));
    }

    [Theory]
    [MemberData(nameof(GetInvalidControlFlowSemanticsData))]
    public void Rejects_invalid_control_flow_semantics(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.ThrowsAny<SemanticException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string, string> GetControlFlowData()
    {
        return new TheoryData<string, string>
        {
            // Ветвления (if..else)
            {
                """
                func int main() {
                    if (true) { print(1); }
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    if (5 > 10) { print(1); } else { print(2); }
                    return 0;
                }
                """,
                "2"
            },
            {
                """
                func int main() {
                    if (true) { 
                        if (false) { print(1); } 
                        else { print(2); } 
                    }
                    return 0;
                }
                """,
                "2"
            },
            {
                """
                func int main() {
                    if (true) { 
                        int x = 5; 
                        print(x); 
                    }
                    return 0;
                }
                """,
                "5"
            },

            // Ветвления с неявным приведением
            {
                """
                func int main() {
                    if (10) { print(1); }
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    if (0) { print(1); } else { print(2); }
                    return 0;
                }
                """,
                "2"
            },
            {
                """
                func int main() {
                    if ("abc") { print(1); }
                    return 0;
                }
                """,
                "1"
            },
        };
    }

    public static TheoryData<string> GetInvalidControlFlowSyntaxData()
    {
        return new TheoryData<string>
        {
            // Пропущены скобки
            {
                """
                func int main() {
                    if true { print(1); }
                    return 0;
                }
                """
            },

            // Пропущены фигурные скобки
            {
                """
                func int main() {
                    if (true) print(1);
                    return 0;
                }
                """
            },
        };
    }

    public static TheoryData<string> GetInvalidControlFlowSemanticsData()
    {
        return new TheoryData<string>
        {
            // Переменная из if недоступна снаружи (область видимости)
            {
                """
                func int main() {
                    if (true) { int x = 1; }
                    print(x);
                    return 0;
                }
                """
            },
        };
    }
}