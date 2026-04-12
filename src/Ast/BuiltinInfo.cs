namespace Ast;

/// <summary>
/// Сигнатура встроенной функции.
/// </summary>
public sealed class BuiltinInfo
{
    public BuiltinInfo(string name, int? fixedArgCount, DataType returnType)
    {
        Name = name;
        FixedArgCount = fixedArgCount;
        ReturnType = returnType;
    }

    /// <summary>
    /// Имя встроенной функции.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Фиксированное количество аргументов (null для variadic-функций).
    /// </summary>
    public int? FixedArgCount { get; }

    /// <summary>
    /// Тип возвращаемого значения.
    /// </summary>
    public DataType ReturnType { get; }
}