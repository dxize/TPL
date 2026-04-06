namespace VirtualMachine.Builtins;

/// <summary>
/// Коды встроенных функций виртуальной машины DEA.
/// </summary>
public enum BuiltinFunctionCode
{
    /// <summary>
    /// Вывод значения на экран.
    /// </summary>
    Print = 0,

    /// <summary>
    /// Длина строки.
    /// </summary>
    Len = 1,

    /// <summary>
    /// Извлечение подстроки.
    /// </summary>
    Substr = 2,

    /// <summary>
    /// Модуль числа.
    /// </summary>
    Abs = 3,

    /// <summary>
    /// Минимальное значение из списка.
    /// </summary>
    Min = 4,

    /// <summary>
    /// Максимальное значение из списка.
    /// </summary>
    Max = 5,
}