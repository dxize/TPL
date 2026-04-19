namespace Ast;

/// <summary>
/// Типы данных языка DEA + E.
/// </summary>
public enum DataType
{
    /// <summary>
    /// Целое число.
    /// </summary>
    Int,

    /// <summary>
    /// Число с плавающей точкой.
    /// </summary>
    Num,

    /// <summary>
    /// Строка.
    /// </summary>
    String,

    /// <summary>
    /// Логический тип данных.
    /// Используется для значений true и false.
    /// </summary>
    Bool,

    /// <summary>
    /// Отсутствие значения.
    /// Используется для конструкций, которые не возвращают результат.
    /// </summary>
    Void,
}