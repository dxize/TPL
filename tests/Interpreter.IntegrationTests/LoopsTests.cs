using Parser;
using Semantics.Exceptions;
using TestsLibrary;

using DeaInterpreter = Interpreter.Interpreter;

namespace Interpreter.IntegrationTests;

public class LoopsTests
{
    [Theory]
    [MemberData(nameof(GetWhileLoopData))]
    public void Can_evaluate_while_loop(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetForLoopData))]
    public void Can_evaluate_for_loop(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetBreakContinueData))]
    public void Can_evaluate_break_continue(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetNestedLoopsData))]
    public void Can_evaluate_nested_loops(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetInvalidLoopSyntaxData))]
    public void Rejects_invalid_loop_syntax(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.Throws<UnexpectedLexemeException>(() => interpreter.Execute(code));
    }

    [Theory]
    [MemberData(nameof(GetInvalidLoopSemanticsData))]
    public void Rejects_invalid_loop_semantics(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.ThrowsAny<SemanticException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string, string> GetWhileLoopData()
    {
        return new TheoryData<string, string>
        {
            {
                """
                func int main() {
                    int i = 5;
                    while (i > 10) {
                        i = i + 1;
                    }
                    return 0;
                }
                """,
                ""
            },
            {
                """
                func int main() {
                    int i = 1;
                    while (i <= 3) {
                        print(i);
                        i = i + 1;
                    }
                    return 0;
                }
                """,
                "123"
            },
            {
                """
                func int main() {
                    int i = 5;
                    while (i > 0) {
                        print(i);
                        i = i - 1;
                    }
                    return 0;
                }
                """,
                "54321"
            },
            {
                """
                func int main() {
                    int i = 3;
                    while (i) {
                        print(i);
                        i = i - 1;
                    }
                    return 0;
                }
                """,
                "321"
            },
            {
                """
                func int main() {
                    string s = "hello";
                    int i = 0;
                    while (s) {
                        print(1);
                        s = "";
                    }
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    int i = 0;
                    while (i) {
                        print(1);
                    }
                    return 0;
                }
                """,
                ""
            },
        };
    }

    public static TheoryData<string, string> GetForLoopData()
    {
        return new TheoryData<string, string>
        {
            {
                """
                func int main() {
                    int i;
                    for (i = 1 to 3) {
                        print(i);
                    }
                    return 0;
                }
                """,
                "123"
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 3 downto 1) {
                        print(i);
                    }
                    return 0;
                }
                """,
                "321"
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 5 to 5) {
                        print(i);
                    }
                    return 0;
                }
                """,
                "5"
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 5 to 3) {
                        print(i);
                    }
                    return 0;
                }
                """,
                ""
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 1 downto 3) {
                        print(i);
                    }
                    return 0;
                }
                """,
                ""
            },
            {
                """
                func int main() {
                    int i;
                    num end = 3.0;
                    for (i = 1 to end) {
                        print(i);
                    }
                    return 0;
                }
                """,
                "123"
            },
        };
    }

    public static TheoryData<string, string> GetBreakContinueData()
    {
        return new TheoryData<string, string>
        {
            {
                """
                func int main() {
                    int i = 0;
                    while (i < 10) {
                        i = i + 1;
                        if (i == 3) {
                            break;
                        }
                        print(i);
                    }
                    return 0;
                }
                """,
                "12"
            },
            {
                """
                func int main() {
                    int i;
                    for (i = 1 to 10) {
                        if (i == 5) {
                            break;
                        }
                        print(i);
                    }
                    return 0;
                }
                """,
                "1234"
            },
            {
                """
                func int main() {
                    int i = 0;
                    while (i < 5) {
                        i = i + 1;
                        if (i == 3) {
                            continue;
                        }
                        print(i);
                    }
                    return 0;
                }
                """,
                "1245"
            },
        };
    }

    public static TheoryData<string, string> GetNestedLoopsData()
    {
        return new TheoryData<string, string>
        {
            {
                """
                func int main() {
                    int i = 0;
                    while (i < 3) {
                        i = i + 1;
                        if (i == 2) {
                            print(99);
                        } else {
                            print(i);
                        }
                    }
                    return 0;
                }
                """,
                "1993"
            },
            {
                """
                func int main() {
                    int i = 5;
                    if (i > 3) {
                        while (i > 3) {
                            print(i);
                            i = i - 1;
                        }
                    } else {
                        print(0);
                    }
                    return 0;
                }
                """,
                "54"
            },
            {
                """
                func int main() {
                    int i = 15;
                    if (i > 10) {
                        if (i > 20) {
                            print(1);
                        } else {
                            print(2);
                        }
                    } else {
                        print(3);
                    }
                    return 0;
                }
                """,
                "2"
            },
        };
    }

    public static TheoryData<string> GetInvalidLoopSyntaxData()
    {
        return new TheoryData<string>
        {
            {
                """
                func int main() {
                    int i = 0;
                    while i < 3 {
                        i = i + 1;
                    }
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    int i;
                    for i = 1 to 3 {
                        print(i);
                    }
                    return 0;
                }
                """
            },
        };
    }

    public static TheoryData<string> GetInvalidLoopSemanticsData()
    {
        return new TheoryData<string>
        {
            {
                """
                func int main() {
                    break;
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    continue;
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    for (i = 1 to 3) {
                        print(i);
                    }
                    return 0;
                }
                """
            },
        };
    }
}