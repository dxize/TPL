using Ast;
using Ast.Expressions;
using Xunit;

namespace Parser.UnitTests;

public class ParserTests
{
    // Точка входа
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

    // Ошибки (точка входа)
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
    public void Throws_when_return_type_is_not_int()
    {
        string code = """
            func string main() {
                return 0;
            }
            """;

        Parser parser = new(code);
        Assert.Throws<UnexpectedLexemeException>(() => parser.ParseProgram());
    }

    [Fact]
    public void Throws_when_return_value_is_not_int_literal()
    {
        string code = """
            func int main() {
                return "dea";
            }
            """;

        Parser parser = new(code);
        Assert.Throws<UnexpectedLexemeException>(() => parser.ParseProgram());
    }

    // Литералы + вывод
    [Fact]
    public void Can_parse_print_with_int_literal()
    {
        string code = """
            func int main() {
                print(2025);
                return 0;
            }
            """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        PrintExpression printExpression = Assert.IsType<PrintExpression>(program.MainFunction.Body[0]);
        Assert.Single(printExpression.Arguments);

        LiteralExpression literal = printExpression.Arguments[0];
        Assert.Equal(DataType.Int, literal.Type);
        Assert.Equal(2025, literal.Value);
    }

    [Fact]
    public void Can_parse_print_with_num_literal()
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
    public void Can_parse_print_with_string_literal()
    {
        string code = """
            func int main() {
                print("hello dea");
                return 0;
            }
            """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        PrintExpression printExpression = Assert.IsType<PrintExpression>(program.MainFunction.Body[0]);
        Assert.Single(printExpression.Arguments);

        LiteralExpression literal = printExpression.Arguments[0];
        Assert.Equal(DataType.String, literal.Type);
        Assert.Equal("hello dea", literal.Value);
    }

    [Fact]
    public void Can_parse_print_with_empty_string()
    {
        string code = """
            func int main() {
                print("");
                return 0;
            }
            """;

        Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();

        PrintExpression printExpression = Assert.IsType<PrintExpression>(program.MainFunction.Body[0]);
        Assert.Single(printExpression.Arguments);

        LiteralExpression literal = printExpression.Arguments[0];
        Assert.Equal(DataType.String, literal.Type);
        Assert.Equal("", literal.Value);
    }

    [Fact]
    public void Can_parse_print_with_multiple_arguments()
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

        LiteralExpression first = printExpression.Arguments[0];
        Assert.Equal(DataType.Int, first.Type);
        Assert.Equal(1, first.Value);

        LiteralExpression second = printExpression.Arguments[1];
        Assert.Equal(DataType.Num, second.Type);
        Assert.Equal(2.5, second.Value);

        LiteralExpression third = printExpression.Arguments[2];
        Assert.Equal(DataType.String, third.Type);
        Assert.Equal("dea", third.Value);
    }

    // Ошибки (Инструкции верхнего уровня)
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

    // Дополнительно: проверка, что неподдерживаемые операторы вызывают ошибку
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