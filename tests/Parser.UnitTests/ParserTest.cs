using Ast;
using Ast.Declarations;
using Ast.Expressions;

namespace Parser.UnitTests;

public class ParserTests
{
    [Fact]
    public void Can_parse_empty_main_with_return()
    {
        string code = """
            func int main() {
                return 0;
            }
            """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        Assert.NotNull(program);
        Assert.Equal("main", program.MainFunction.Name);
        Assert.Equal(DataType.Int, program.MainFunction.ReturnType);
        Assert.Single(program.MainFunction.Body);

        ReturnExpression returnExpression = Assert.IsType<ReturnExpression>(program.MainFunction.Body[0]);
        Assert.Equal(DataType.Int, returnExpression.Value.Type);
        Assert.Equal(0, returnExpression.Value.Value);
    }

    [Fact]
    public void Can_parse_print_with_single_int_literal()
    {
        string code = """
            func int main() {
                print(123);
                return 0;
            }
            """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        Assert.Equal(2, program.MainFunction.Body.Count);

        PrintExpression printExpression = Assert.IsType<PrintExpression>(program.MainFunction.Body[0]);
        Assert.Single(printExpression.Arguments);

        LiteralExpression literal = printExpression.Arguments[0];
        Assert.Equal(DataType.Int, literal.Type);
        Assert.Equal(123, literal.Value);
    }

    [Fact]
    public void Can_parse_print_with_single_num_literal()
    {
        string code = """
            func int main() {
                print(3.14);
                return 0;
            }
            """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        PrintExpression printExpression = Assert.IsType<PrintExpression>(program.MainFunction.Body[0]);
        Assert.Single(printExpression.Arguments);

        LiteralExpression literal = printExpression.Arguments[0];
        Assert.Equal(DataType.Num, literal.Type);
        Assert.Equal(3.14, literal.Value);
    }

    [Fact]
    public void Can_parse_print_with_single_string_literal()
    {
        string code = """
            func int main() {
                print("hello");
                return 0;
            }
            """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        PrintExpression printExpression = Assert.IsType<PrintExpression>(program.MainFunction.Body[0]);
        Assert.Single(printExpression.Arguments);

        LiteralExpression literal = printExpression.Arguments[0];
        Assert.Equal(DataType.String, literal.Type);
        Assert.Equal("hello", literal.Value);
    }

    [Fact]
    public void Can_parse_print_with_multiple_literals()
    {
        string code = """
            func int main() {
                print(1, 2.5, "abc");
                return 0;
            }
            """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        PrintExpression printExpression = Assert.IsType<PrintExpression>(program.MainFunction.Body[0]);
        Assert.Equal(3, printExpression.Arguments.Count);

        Assert.Equal(DataType.Int, printExpression.Arguments[0].Type);
        Assert.Equal(1, printExpression.Arguments[0].Value);

        Assert.Equal(DataType.Num, printExpression.Arguments[1].Type);
        Assert.Equal(2.5, printExpression.Arguments[1].Value);

        Assert.Equal(DataType.String, printExpression.Arguments[2].Type);
        Assert.Equal("abc", printExpression.Arguments[2].Value);
    }

    [Fact]
    public void Can_parse_empty_print()
    {
        string code = """
            func int main() {
                print();
                return 0;
            }
            """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        PrintExpression printExpression = Assert.IsType<PrintExpression>(program.MainFunction.Body[0]);
        Assert.Empty(printExpression.Arguments);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("2025")]
    public void Can_parse_return_int_literal(string value)
    {
        string code = $$"""
        func int main() {
            return {{value}};
        }
        """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        ReturnExpression returnExpression = Assert.IsType<ReturnExpression>(program.MainFunction.Body[0]);
        Assert.Equal(DataType.Int, returnExpression.Value.Type);
        Assert.Equal(int.Parse(value), returnExpression.Value.Value);
    }

    [Fact]
    public void Throws_when_main_is_missing()
    {
        string code = """
            func int notmain() {
                return 0;
            }
            """;

        Parser parser = new(code);

        Assert.Throws<UnexpectedLexemeException>(() => parser.ParseProgram());
    }

    [Fact]
    public void Throws_when_return_is_not_int_literal()
    {
        string code = """
            func int main() {
                return "abc";
            }
            """;

        Parser parser = new(code);

        Assert.Throws<UnexpectedLexemeException>(() => parser.ParseProgram());
    }

    [Fact]
    public void Throws_when_statement_is_not_supported_in_iteration_2()
    {
        string code = """
            func int main() {
                input(x);
                return 0;
            }
            """;

        Parser parser = new(code);

        Assert.Throws<UnexpectedLexemeException>(() => parser.ParseProgram());
    }
}