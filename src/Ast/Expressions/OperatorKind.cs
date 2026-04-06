namespace Ast.Expressions;

/// <summary>
/// Виды операторов выражений языка DEA.
/// </summary>
public enum OperatorKind
{
    /// <summary>
    /// Сложение (+).
    /// </summary>
    Plus,

    /// <summary>
    /// Вычитание (-).
    /// </summary>
    Minus,

    /// <summary>
    /// Умножение (*).
    /// </summary>
    Multiply,

    /// <summary>
    /// Деление (/).
    /// </summary>
    Divide,

    /// <summary>
    /// Целочисленное деление (//).
    /// </summary>
    IntegerDivide,

    /// <summary>
    /// Остаток от деления (%).
    /// </summary>
    Modulo,

    /// <summary>
    /// Возведение в степень (^).
    /// </summary>
    Power,
}