namespace VirtualMachine.Instructions;

/// <summary>
/// Коды инструкций виртуальной машины DEA.
/// </summary>
public enum InstructionCode
{
    /// <summary>
    /// Поместить значение на стек.
    /// </summary>
    Push,

    /// <summary>
    /// Объявить переменную.
    /// </summary>
    DefineVar,

    /// <summary>
    /// Загрузить значение переменной на стек.
    /// </summary>
    LoadVar,

    /// <summary>
    /// Сохранить значение в переменную.
    /// </summary>
    StoreVar,

    /// <summary>
    /// Ввести значение переменной.
    /// </summary>
    InputVar,

    /// <summary>
    /// Сложение.
    /// </summary>
    Add,

    /// <summary>
    /// Вычитание.
    /// </summary>
    Subtract,

    /// <summary>
    /// Умножение.
    /// </summary>
    Multiply,

    /// <summary>
    /// Деление.
    /// </summary>
    Divide,

    /// <summary>
    /// Целочисленное деление.
    /// </summary>
    IntegerDivide,

    /// <summary>
    /// Остаток от деления.
    /// </summary>
    Modulo,

    /// <summary>
    /// Возведение в степень.
    /// </summary>
    Power,

    /// <summary>
    /// Унарное отрицание.
    /// </summary>
    Negate,

    /// <summary>
    /// Вызов встроенной функции.
    /// </summary>
    CallBuiltin,

    /// <summary>
    /// Завершение программы.
    /// </summary>
    Halt,
}