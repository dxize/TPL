using Parser;
using Semantics.Exceptions;
using TestsLibrary;

using DeaInterpreter = Interpreter.Interpreter;

namespace Interpreter.IntegrationTests;

public class EntryPointTests
{
    [Theory]
    [MemberData(nameof(GetValidEntryPointsData))]
    public void Can_execute_valid_entry_points(string code, string expectedOutput)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        interpreter.Execute(code);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(GetInvalidSyntaxEntryPointsData))]
    public void Rejects_invalid_syntax_entry_points(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.Throws<UnexpectedLexemeException>(() => interpreter.Execute(code));
    }

    [Theory]
    [MemberData(nameof(GetInvalidSemanticEntryPointsData))]
    public void Rejects_invalid_semantic_entry_points(string code)
    {
        FakeEnvironment environment = new();
        DeaInterpreter interpreter = new(environment);

        Assert.ThrowsAny<SemanticException>(() => interpreter.Execute(code));
    }

    public static TheoryData<string, string> GetValidEntryPointsData()
    {
        return new TheoryData<string, string>
        {
            {
                """
                func int main() {
                    return 0;
                }
                """,
                ""
            },
            {
                """
                func int main() {
                    print(1);
                    return 0;
                }
                """,
                "1"
            },
            {
                """
                func int main() {
                    int x = 10;
                    return 0;
                }
                """,
                ""
            },
        };
    }

    public static TheoryData<string> GetInvalidSyntaxEntryPointsData()
    {
        return new TheoryData<string>
        {
            {
                """
                func int start() {
                    return 0;
                }
                """
            },
            {
                """
                func string main() {
                    return 0;
                }
                """
            },
            {
                """
                func int main() {
                    return 0;
                }

                func int other() {
                    return 0;
                }
                """
            },
        };
    }

    public static TheoryData<string> GetInvalidSemanticEntryPointsData()
    {
        return new TheoryData<string>
        {
            {
                """
                func int main() {
                    print(1);
                }
                """
            },
            {
                """
                func int main() {
                    return "dea";
                }
                """
            },
        };
    }
}