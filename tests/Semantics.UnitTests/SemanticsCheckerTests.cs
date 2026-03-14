using Semantics;
using Semantics.Exceptions;
using DeaParser = global::Parser.Parser;

namespace Semantics.UnitTests;

public class SemanticsCheckerTests
{
    [Fact]
    public void Accepts_valid_iteration2_program()
    {
        string code = """
            func int main() {
                print(1, 2.5, "dea");
                return 0;
            }
            """;

        Ast.ProgramNode program = new DeaParser(code).ParseProgram();
        SemanticsChecker checker = new();

        checker.Check(program);
    }

    [Fact]
    public void Rejects_non_main_function_name()
    {
        string code = """
            func int start() {
                return 0;
            }
            """;

        DeaParser parser = new(code);
        Assert.Throws<Parser.UnexpectedLexemeException>(() => parser.ParseProgram());
    }

    [Fact]
    public void Rejects_statements_after_return()
    {
        string code = """
            func int main() {
                return 0;
                print(1);
            }
            """;

        Ast.ProgramNode program = new DeaParser(code).ParseProgram();
        SemanticsChecker checker = new();

        InvalidExpressionException exception = Assert.Throws<InvalidExpressionException>(() => checker.Check(program));
        Assert.Contains("После return", exception.Message);
    }
}