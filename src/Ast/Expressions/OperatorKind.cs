namespace Ast.Expressions;

/// <summary>
/// Виды операторов выражений языка DEA + E.
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

    /// <summary>
    /// Проверка на равенство (==).
    /// </summary>
    Equal,

    /// <summary>
    /// Проверка на неравенство (!=).
    /// </summary>
    NotEqual,

    /// <summary>
    /// Проверка "меньше" (<).
    /// </summary>
    Less,

    /// <summary>
    /// Проверка "меньше или равно" (<=).
    /// </summary>
    LessOrEqual,

    /// <summary>
    /// Проверка "больше" (>).
    /// </summary>
    Greater,

    /// <summary>
    /// Проверка "больше или равно" (>=).
    /// </summary>
    GreaterOrEqual,

    /// <summary>
    /// Логическое И (&&).
    /// </summary>
    And,

    /// <summary>
    /// Логическое ИЛИ (||).
    /// </summary>
    Or,

    /// <summary>
    /// Логическое НЕ (!).
    /// </summary>
    Not,
}