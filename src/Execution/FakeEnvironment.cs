namespace Execution;

/// <summary>
/// Фейковое окружение для тестирования.
/// Симулирует ввод/вывод без реального использования консоли.
/// </summary>
public class FakeEnvironment : IEnvironment
{
    private readonly List<double> _results = [];
    private readonly Queue<double> _inputQueue = new();

    /// <summary>
    /// Создает фейковое окружение с предопределенными входными данными.
    /// </summary>
    /// <param name="inputs">Входные данные для симуляции</param>
    public FakeEnvironment(params double[] inputs)
    {
        foreach (var input in inputs)
        {
            _inputQueue.Enqueue(input);
        }
    }

    /// <summary>
    /// Получает список всех добавленных результатов.
    /// </summary>
    public IReadOnlyList<double> Results => _results;

    /// <summary>
    /// Читает следующее значение из очереди входных данных.
    /// </summary>
    public double ReadInput()
    {
        if (_inputQueue.Count == 0)
        {
            throw new InvalidOperationException("No input available in test environment");
        }

        return _inputQueue.Dequeue();
    }

    /// <summary>
    /// Добавляет результат в список результатов.
    /// </summary>
    public void AddResult(double result)
    {
        _results.Add(result);
    }
}
