namespace Execution;

/// <summary>
/// Реализация IEnvironment для работы с консолью.
/// </summary>
public class ConsoleEnvironment : IEnvironment
{
    /// <summary>
    /// Читает число из консоли.
    /// </summary>
    public double ReadInput()
    {
        while (true)
        {
            string? input = Console.ReadLine();
            if (input != null && double.TryParse(input, out double result))
            {
                return result;
            }

            Console.WriteLine("Ошибка! Введите корректное число:");
        }
    }

    /// <summary>
    /// Выводит результат в консоль.
    /// </summary>
    public void AddResult(double result)
    {
        Console.WriteLine(result);
    }
}
