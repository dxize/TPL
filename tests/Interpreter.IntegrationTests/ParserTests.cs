using Ast;
using Ast.Expressions;

using Parser;

namespace Interpreter.IntegrationTests;

public class ParserTests
{
    [Theory]
    [MemberData(nameof(GetValidProgramData))]
    public void Can_parse_valid_programs(string code, Action<ProgramNode> assertProgram)
    {
        Parser.Parser parser = new(code);
        ProgramNode program = parser.ParseProgram();
        assertProgram(program);
    }

    public static TheoryData<string, Action<ProgramNode>> GetValidProgramData()
    {
        return new TheoryData<string, Action<ProgramNode>>
        {
            {
                """
                func int main() {
                    return 0;
                }
                """,
                program =>
                {
                    Assert.Equal("main", program.MainFunction.Name);
                    Assert.Equal(DataType.Int, program.MainFunction.ReturnType);
                    Assert.Single(program.MainFunction.Body);

                    ReturnExpression ret = Assert.IsType<ReturnExpression>(program.MainFunction.Body[0]);
                    LiteralExpression literal = Assert.IsType<LiteralExpression>(ret.Value);
                    Assert.Equal(DataType.Int, literal.Type);
                    Assert.Equal(0, literal.Value);
                }
            },
            {
                """
                func int main() {
                    int x = 2;
                    x = x + 5;
                    print(x, "!");
                    return x;
                }
                """,
                program =>
                {
                    Assert.Equal(4, program.MainFunction.Body.Count);

                    VariableDeclarationExpression variable = Assert.IsType<VariableDeclarationExpression>(program.MainFunction.Body[0]);
                    Assert.Equal(DataType.Int, variable.Type);
                    Assert.Equal("x", variable.Name);
                    Assert.NotNull(variable.Initializer);

                    AssignmentExpression assignment = Assert.IsType<AssignmentExpression>(program.MainFunction.Body[1]);
                    Assert.Equal("x", assignment.Name);
                    BinaryExpression sum = Assert.IsType<BinaryExpression>(assignment.Value);
                    Assert.Equal(OperatorKind.Plus, sum.OperatorKind);

                    PrintExpression print = Assert.IsType<PrintExpression>(program.MainFunction.Body[2]);
                    Assert.Equal(2, print.Arguments.Count);
                    Assert.IsType<IdentifierExpression>(print.Arguments[0]);
                    Assert.IsType<LiteralExpression>(print.Arguments[1]);

                    ReturnExpression ret = Assert.IsType<ReturnExpression>(program.MainFunction.Body[3]);
                    Assert.IsType<IdentifierExpression>(ret.Value);
                }
            },
            {
                """
                func int main() {
                    const num pi = 3.14;
                    string name;
                    input(name);
                    print(substr(name, 0, len(name)));
                    return 0;
                }
                """,
                program =>
                {
                    Assert.Equal(5, program.MainFunction.Body.Count);

                    ConstantDeclarationExpression constant = Assert.IsType<ConstantDeclarationExpression>(program.MainFunction.Body[0]);
                    Assert.Equal(DataType.Num, constant.Type);
                    Assert.Equal("pi", constant.Name);
                    LiteralExpression pi = Assert.IsType<LiteralExpression>(constant.Initializer);
                    Assert.Equal(3.14, pi.Value);

                    VariableDeclarationExpression variable = Assert.IsType<VariableDeclarationExpression>(program.MainFunction.Body[1]);
                    Assert.Equal(DataType.String, variable.Type);
                    Assert.Null(variable.Initializer);

                    InputExpression input = Assert.IsType<InputExpression>(program.MainFunction.Body[2]);
                    Assert.Equal("name", input.VariableName);

                    PrintExpression print = Assert.IsType<PrintExpression>(program.MainFunction.Body[3]);
                    Assert.Single(print.Arguments);
                    CallExpression outerCall = Assert.IsType<CallExpression>(print.Arguments[0]);
                    Assert.Equal("substr", outerCall.Name);
                    Assert.Equal(3, outerCall.Arguments.Count);
                    Assert.IsType<IdentifierExpression>(outerCall.Arguments[0]);
                    Assert.IsType<LiteralExpression>(outerCall.Arguments[1]);
                    CallExpression nestedCall = Assert.IsType<CallExpression>(outerCall.Arguments[2]);
                    Assert.Equal("len", nestedCall.Name);
                }
            },
            {
                """
                func int main() {
                    return -2 ^ 3 ^ 2;
                }
                """,
                program =>
                {
                    ReturnExpression ret = Assert.IsType<ReturnExpression>(program.MainFunction.Body[0]);
                    UnaryExpression unaryMinus = Assert.IsType<UnaryExpression>(ret.Value);
                    Assert.Equal(OperatorKind.Minus, unaryMinus.OperatorKind);

                    BinaryExpression power = Assert.IsType<BinaryExpression>(unaryMinus.Operand);
                    Assert.Equal(OperatorKind.Power, power.OperatorKind);
                    LiteralExpression left = Assert.IsType<LiteralExpression>(power.Left);
                    Assert.Equal(2, left.Value);

                    BinaryExpression rightPower = Assert.IsType<BinaryExpression>(power.Right);
                    Assert.Equal(OperatorKind.Power, rightPower.OperatorKind);
                }
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidProgramData))]
    public void Throws_for_invalid_programs(string code)
    {
        Parser.Parser parser = new(code);
        Assert.Throws<UnexpectedLexemeException>(() => parser.ParseProgram());
    }

    public static TheoryData<string> GetInvalidProgramData()
    {
        return new TheoryData<string>
        {
            // Нет main
            """
            func int start() {
                return 0;
            }
            """,
            // Неверный тип возврата main
            """
            func string main() {
                return 0;
            }
            """,
            // print без аргументов
            """
            func int main() {
                print();
                return 0;
            }
            """,
            // Константа без инициализации
            """
            func int main() {
                const int x;
                return 0;
            }
            """,
            // input ожидает идентификатор
            """
            func int main() {
                input(1);
                return 0;
            }
            """,
            // Пропущен разделитель инструкции
            """
            func int main() {
                int x = 1
                return 0;
            }
            """,
            // Лишний токен после программы
            """
            func int main() {
                return 0;
            }
            func int other() { return 0; }
            """,
        };
    }
}