namespace Ast.Expressions;

public enum OperatorKind
{
    // Арифметические
    Plus,
    Minus,
    Multiply,
    Divide,
    IntegerDivide,
    Modulo,
    Power,

    // Операторы сравнения
    Less,
    LessOrEqual,
    Greater,
    GreaterOrEqual,
    Equal,
    NotEqual,
}
