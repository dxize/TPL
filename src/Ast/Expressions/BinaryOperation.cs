namespace Ast.Expressions;

public enum BinaryOperation
{
    /// <summary>
    /// Операция сложения.
    /// </summary>
    Plus,

    /// <summary>
    /// Операция вычитания.
    /// </summary>
    Minus,

    /// <summary>
    /// Операция умножения.
    /// </summary>
    Multiply,

    /// <summary>
    /// Операция деления "/".
    /// </summary>
    Divide,

    /// <summary>
    /// Операция целочисленного деления "//".
    /// </summary>
    IntegerDivide,

    /// <summary>
    /// Операция деления "%".
    /// </summary>
    Modulo,

    /// <summary>
    /// Операция возведения в степень "^".
    /// </summary>
    Power,

    /// <summary>
    /// Операция сравнения меньше "<".
    /// </summary>
    LessThan,

    /// <summary>
    /// Операция сравнения больше ">".
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Операция сравнения меньше или равно "<=".
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Операция сравнения больше или равно ">=".
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Операция сравнения равно "==".
    /// </summary>
    Equal,

    /// <summary>
    /// Операция сравнения не равно "!=".
    /// </summary>
    NotEqual,

    /// <summary>
    /// Операция логическое И "&&".
    /// </summary>
    LogicalAnd,

    /// <summary>
    /// Операция логическое ИЛИ "||".
    /// </summary>
    LogicalOr,
}