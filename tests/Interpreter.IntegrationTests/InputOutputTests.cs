using Parser;
using Semantics.Exceptions;
using TestsLibrary;

using DeaInterpreter = Interpreter.Interpreter;

namespace Interpreter.IntegrationTests;

public class InputOutputTests
{
    [Theory]
    [MemberData(nameof(GetEvaluateOutputData))]
    public void Can_evaluate_output_statements(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetEvaluateInputData))]
    public void Can_evaluate_input_statements(string code, string input, string expectedOutput)
    {
        FakeEnvironment environment = new();
        environment.AddInput(input);

        DeaInterpreter interpreter = new(environment);
        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetInvalidOutputSyntaxData))]
    public void Rejects_invalid_output_syntax(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.Throws<UnexpectedLexemeException>(() => interpreter.Execute(code));
    }

    [Theory]
    [MemberData(nameof(GetInvalidInputSyntaxData))]
    public void Rejects_invalid_input_syntax(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.Throws<UnexpectedLexemeException>(() => interpreter.Execute(code));
    }

    [Theory]
    [MemberData(nameof(GetInvalidInputSemanticData))]
    public void Rejects_invalid_input_semantics(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.ThrowsAny<SemanticException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string, string> GetEvaluateOutputData()
    {
        return new TheoryData<string, string>
        {
            // Вывод
            {
                """
                func int main() {
                    print("");
                    return 0;
                }
                """,
                ""
            },
            {
                """
                func int main() {
                    print(1, 2.5, "dea");
                    return 0;
                }
                """,
                "12.5dea"
            },
        };
    }

    public static TheoryData<string, string, string> GetEvaluateInputData()
    {
        return new TheoryData<string, string, string>
        {
            // Ввод
            {
                """
                func int main() {
                    string name;
                    input(name);
                    return 0;
                }
                """,
                "dea",
                ""
            },
            {
                """
                func int main() {
                    int x;
                    input(x);
                    print(x);
                    return 0;
                }
                """,
                "42",
                "42"
            },
            {
                """
                func int main() {
                    num x;
                    input(x);
                    print(x);
                    return 0;
                }
                """,
                "3.14",
                "3.14"
            },
            {
                """
                func int main() {
                    string name;
                    input(name);
                    print(name);
                    return 0;
                }
                """,
                "dea",
                "dea"
            },
        };
    }

    public static TheoryData<string> GetInvalidOutputSyntaxData()
    {
        return new TheoryData<string>
        {
            // Ошибки (Вывод, синтаксис)
            {
                """
                func int main() {
                    print();
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    print(1,);
                    return 0;
                }
                """
            },
        };
    }

    public static TheoryData<string> GetInvalidInputSyntaxData()
    {
        return new TheoryData<string>
        {
            // Ошибки (Ввод)
            {
                """
                func int main() {
                    input(1);
                    return 0;
                }
                """
            },
        };
    }

    public static TheoryData<string> GetInvalidInputSemanticData()
    {
        return new TheoryData<string>
        {
            // Ошибки (Ввод, семантика)
            {
                """
                func int main() {
                    const string name = "dea";
                    input(name);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    input(name);
                    return 0;
                }
                """
            },
        };
    }
}