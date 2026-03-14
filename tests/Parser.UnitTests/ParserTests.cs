using Ast;
using Ast.Expressions;

namespace Parser.UnitTests;

public class ParserTests
{
    [Fact]
    public void Can_parse_minimal_main_program()
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
        Assert.Equal(0, returnExpression.Value.Value);
    }

    [Fact]
    public void Can_parse_print_with_all_supported_iteration2_literal_types()
    {
        string code = """
            func int main() {
                print(1, 2.5, "dea");
                return 0;
            }
            """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        PrintExpression printExpression = Assert.IsType<PrintExpression>(program.MainFunction.Body[0]);
        Assert.Equal(3, printExpression.Arguments.Count);
        Assert.Equal(DataType.Int, printExpression.Arguments[0].Type);
        Assert.Equal(DataType.Num, printExpression.Arguments[1].Type);
        Assert.Equal(DataType.String, printExpression.Arguments[2].Type);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("42", 42)]
    [InlineData("2026", 2026)]
    public void Can_parse_return_int_literal(string literal, int expected)
    {
        string code = $$"""
            func int main() {
                return {{literal}};
            }
            """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        ReturnExpression returnExpression = Assert.IsType<ReturnExpression>(program.MainFunction.Body[0]);
        Assert.Equal(DataType.Int, returnExpression.Value.Type);
        Assert.Equal(expected, returnExpression.Value.Value);
    }

    [Fact]
    public void Throws_when_main_is_missing()
    {
        string code = """
            func int start() {
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
                return "dea";
            }
            """;

        Parser parser = new(code);
        Assert.Throws<UnexpectedLexemeException>(() => parser.ParseProgram());
    }

    [Fact]
    public void Throws_when_print_has_no_arguments()
    {
        string code = """
            func int main() {
                print();
                return 0;
            }
            """;

        Parser parser = new(code);
        Assert.Throws<UnexpectedLexemeException>(() => parser.ParseProgram());
    }

    [Fact]
    public void Throws_when_statement_is_not_supported_in_iteration2()
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