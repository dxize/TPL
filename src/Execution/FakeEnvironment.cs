using Ast;

namespace Execution;

/// <summary>
/// Фейковое окружение для тестирования.
/// Симулирует ввод/вывод без реального использования консоли.
/// </summary>
public class FakeEnvironment : IEnvironment
{
    private readonly List<object> _results = [];
    private readonly Queue<object> _inputQueue = new();

    /// <summary>
    /// Создает фейковое окружение с предопределенными входными данными.
    /// </summary>
    /// <param name="inputs">Входные данные для симуляции</param>
    public FakeEnvironment(params object[] inputs)
    {
        foreach (object input in inputs)
        {
            _inputQueue.Enqueue(input);
        }
    }

    /// <summary>
    /// Получает список всех добавленных результатов.
    /// </summary>
    public IReadOnlyList<object> Results => _results;

    /// <summary>
    /// Читает следующее значение из очереди входных данных.
    /// </summary>
    public object ReadInput(DataType type)
    {
        if (_inputQueue.Count == 0)
        {
            throw new InvalidOperationException("No input available in test environment");
        }

        object input = _inputQueue.Dequeue();
        
        // Basic conversion to ensure type safety in tests if needed
        return type switch
        {
            DataType.Int => Convert.ToInt32(input),
            DataType.Num => Convert.ToDouble(input),
            DataType.String => input.ToString()!,
            DataType.Bool => Convert.ToBoolean(input),
            _ => input
        };
    }

    /// <summary>
    /// Добавляет результат в список результатов.
    /// </summary>
    public void AddResult(object result)
    {
        _results.Add(result);
    }
}
