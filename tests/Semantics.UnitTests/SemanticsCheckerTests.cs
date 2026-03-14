using Ast;
using Semantics.Exceptions;

using DeaParser = global::Parser.Parser;

namespace Semantics.UnitTests;

public class SemanticsCheckerTests
{
    // Корректные программы
    [Fact]
    public void Accepts_minimal_main()
    {
        string code = """
            func int main() {
                return 0;
            }
            """;
        CheckProgram(code);
    }

    [Fact]
    public void Accepts_main_with_print_int()
    {
        string code = """
            func int main() {
                print(42);
                return 0;
            }
            """;
        CheckProgram(code);
    }

    [Fact]
    public void Accepts_main_with_print_num()
    {
        string code = """
            func int main() {
                print(3.14);
                return 0;
            }
            """;
        CheckProgram(code);
    }

    [Fact]
    public void Accepts_main_with_print_string()
    {
        string code = """
            func int main() {
                print("hello");
                return 0;
            }
            """;
        CheckProgram(code);
    }

    [Fact]
    public void Accepts_main_with_print_multiple_arguments()
    {
        string code = """
            func int main() {
                print(1, 2.5, "dea");
                return 0;
            }
            """;
        CheckProgram(code);
    }

    [Fact]
    public void Accepts_main_with_multiple_prints()
    {
        string code = """
            func int main() {
                print(1);
                print(2);
                return 0;
            }
            """;
        CheckProgram(code);
    }

    // Ошибки main
    [Fact]
    public void Rejects_empty_main_body()
    {
        string code = """
            func int main() {
            }
            """;
        Assert.Throws<InvalidExpressionException>(() => CheckProgram(code));
    }

    [Fact]
    public void Rejects_missing_return()
    {
        string code = """
            func int main() {
                print(1);
            }
            """;
        Assert.Throws<InvalidExpressionException>(() => CheckProgram(code));
    }

    [Fact]
    public void Rejects_statement_after_return()
    {
        string code = """
            func int main() {
                return 0;
                print(1);
            }
            """;
        InvalidExpressionException ex = Assert.Throws<InvalidExpressionException>(() => CheckProgram(code));
        Assert.Contains("После return", ex.Message);
    }

    private void CheckProgram(string code)
    {
        ProgramNode program = new DeaParser(code).ParseProgram();
        SemanticsChecker checker = new();
        checker.Check(program);
    }
}