using Ast;
using System.Globalization;

namespace Execution;

/// <summary>
/// Реализация IEnvironment для работы с консолью.
/// </summary>
public class ConsoleEnvironment : IEnvironment
{
    /// <summary>
    /// Читает значение указанного типа из консоли.
    /// </summary>
    public object ReadInput(DataType type)
    {
        while (true)
        {
            string? input = Console.ReadLine();
            if (input == null) return null!;

            switch (type)
            {
                case DataType.Int:
                    if (int.TryParse(input, out int intResult)) return intResult;
                    break;
                case DataType.Num:
                    if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double numResult)) return numResult;
                    break;
                case DataType.String:
                    return input;
                case DataType.Bool:
                    if (bool.TryParse(input, out bool boolResult)) return boolResult;
                    if (input == "1") return true;
                    if (input == "0") return false;
                    break;
            }

            Console.WriteLine($"Ошибка! Введите корректное значение типа {type}:");
        }
    }

    /// <summary>
    /// Выводит результат в консоль.
    /// </summary>
    public void AddResult(object result)
    {
        Console.Write(result?.ToString());
    }
}
