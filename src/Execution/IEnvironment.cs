using Ast;

namespace Execution;

/// <summary>
/// Представляет окружение для выполнения программы.
/// Отвечает за ввод/вывод данных.
/// </summary>
public interface IEnvironment
{
    /// <summary>
    /// Читает значение указанного типа из входного потока.
    /// </summary>
    object ReadInput(DataType type);

    /// <summary>
    /// Добавляет результат в выходной поток.
    /// </summary>
    void AddResult(object result);
}
