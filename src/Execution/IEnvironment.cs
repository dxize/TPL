namespace Execution;

/// <summary>
/// Представляет окружение для выполнения программы.
/// Отвечает за ввод/вывод данных.
/// </summary>
public interface IEnvironment
{
    /// <summary>
    /// Читает число из входного потока.
    /// </summary>
    /// <returns>Прочитанное число</returns>
    double ReadInput();

    /// <summary>
    /// Добавляет результат в выходной поток.
    /// </summary>
    /// <param name="result">Значение для вывода</param>
    void AddResult(double result);
}
