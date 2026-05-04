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
    /// Вызов пользовательской функции или процедуры.
    /// </summary>
    CallUser,

    /// <summary>
    /// Возврат из функции или процедуры.
    /// </summary>
    Return,

    /// <summary>
    /// Безусловный переход.
    /// </summary>
    Jump,

    /// <summary>
    /// Переход, если условие ложно.
    /// </summary>
    JumpIfFalse,

    /// <summary>
    /// Преобразовать значение к bool.
    /// </summary>
    ToBool,

    /// <summary>
    /// Логическое отрицание.
    /// </summary>
    Not,

    /// <summary>
    /// Проверка на равенство.
    /// </summary>
    Equal,

    /// <summary>
    /// Проверка на неравенство.
    /// </summary>
    NotEqual,

    /// <summary>
    /// Проверка "меньше".
    /// </summary>
    Less,

    /// <summary>
    /// Проверка "меньше или равно".
    /// </summary>
    LessOrEqual,

    /// <summary>
    /// Проверка "больше".
    /// </summary>
    Greater,

    /// <summary>
    /// Проверка "больше или равно".
    /// </summary>
    GreaterOrEqual,

    /// <summary>
    /// Завершение программы.
    /// </summary>
    Halt,

    /// <summary>
    /// Открыть новую лексическую область видимости.
    /// </summary>
    EnterScope,

    /// <summary>
    /// Закрыть текущую лексическую область видимости (удалить локальные переменные блока).
    /// </summary>
    LeaveScope,
}