using Parser;
using Semantics.Exceptions;
using TestsLibrary;

using DeaInterpreter = Interpreter.Interpreter;

namespace Interpreter.IntegrationTests;

public class VariablesTests
{
    [Theory]
    [MemberData(nameof(GetEvaluateVariablesAndConstantsData))]
    public void Can_evaluate_variables_and_constants(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetInvalidVariableSyntaxData))]
    public void Rejects_invalid_variable_syntax(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.Throws<UnexpectedLexemeException>(() => interpreter.Execute(code));
    }

    [Theory]
    [MemberData(nameof(GetInvalidVariableSemanticsData))]
    public void Rejects_invalid_variable_semantics(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.ThrowsAny<SemanticException>(() => interpreter.Execute(code));
    }

    [Theory]
    [MemberData(nameof(GetInvalidVariableRuntimeData))]
    public void Rejects_invalid_variable_runtime(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.Throws<RuntimeException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string, string> GetEvaluateVariablesAndConstantsData()
    {
        return new TheoryData<string, string>
        {
            {
                """
                func int main() {
                    int x = 10;
                    return 0;
                }
                """,
                ""
            },
            {
                """
                func int main() {
                    num x = 3.14;
                    return 0;
                }
                """,
                ""
            },
            {
                """
                func int main() {
                    string s = "dea";
                    return 0;
                }
                """,
                ""
            },
            {
                """
                func int main() {
                    string name;
                    return 0;
                }
                """,
                ""
            },
            {
                """
                func int main() {
                    int x = 1;
                    x = 2;
                    print(x);
                    return 0;
                }
                """,
                "2"
            },
            {
                """
                func int main() {
                    int x = 2;
                    int y = 3;
                    print(x + y);
                    return 0;
                }
                """,
                "5"
            },
            {
                """
                func int main() {
                    int x = 10;
                    print(x);
                    return 0;
                }
                """,
                "10"
            },
            {
                """
                func int main() {
                    const int x = 10;
                    print(x);
                    return 0;
                }
                """,
                "10"
            },
            {
                """
                func int main() {
                    const num pi = 3.14;
                    print(pi);
                    return 0;
                }
                """,
                "3.14"
            },
            {
                """
                func int main() {
                    const string name = "dea";
                    print(name);
                    return 0;
                }
                """,
                "dea"
            },
            {
                """
                func int main() {
                    const int x = 10;
                    print(x + 5);
                    return 0;
                }
                """,
                "15"
            },
            {
                """
                int x = 10;

                func int main() {
                    print(x);
                    return 0;
                }
                """,
                "10"
            },
            {
                """
                const int x = 10;

                func int main() {
                    print(x);
                    return 0;
                }
                """,
                "10"
            },

            // bool
            {
                """
                func int main() {
                    bool b = true;
                    print(b);
                    return 0;
                }
                """,
                "true"
            },
            {
                """
                func int main() {
                    const bool flag = false;
                    print(flag);
                    return 0;
                }
                """,
                "false"
            },
        };
    }

    public static TheoryData<string> GetInvalidVariableSyntaxData()
    {
        return new TheoryData<string>
        {
            {
                """
                func int main() {
                    int x = 0;
                    int y = 0;
                    x = y = 0;
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    int x = 10;
                    10 = x;
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    const int x;
                    return 0;
                }
                """
            },
        };
    }

    public static TheoryData<string> GetInvalidVariableSemanticsData()
    {
        return new TheoryData<string>
        {
            {
                """
                func int main() {
                    print(x);
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    int x = "dea";
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    int x = 10;
                    x = "dea";
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    const int x = 10;
                    x = 20;
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    int x = 1;
                    int x = 2;
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    const int x = 1;
                    const int x = 2;
                    return 0;
                }
                """
            },
            {
                """
                int x = 10;

                func int main() {
                    int x = 20;
                    return 0;
                }
                """
            },
            {
                """
                const int x = 10;

                func int main() {
                    int x = 20;
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    int len = 10;
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    const int len = 10;
                    return 0;
                }
                """
            },

            // bool
            {
                """
                func int main() {
                    bool b = 1;
                    return 0;
                }
                """
            },
        };
    }

    public static TheoryData<string> GetInvalidVariableRuntimeData()
    {
        return new TheoryData<string>
        {
            {
                """
                func int main() {
                    int x;
                    print(x);
                    return 0;
                }
                """
            },
        };
    }
}