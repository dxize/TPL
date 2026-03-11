using Execution;

namespace Interpreter;

/// <summary>
/// Интерпретатор языка DEA
/// </summary>
public class Interpreter
{
    private readonly IEnvironment _environment;

    public Interpreter(IEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Выполняет программу на языке DEA
    /// </summary>
    /// <param name="sourceCode">Исходный код программы</param>
    public void Execute(string sourceCode)
    {
        if (string.IsNullOrEmpty(sourceCode))
        {
            throw new ArgumentException("Source code cannot be null or empty", nameof(sourceCode));
        }

        // Создаем контекст и парсер
        Context context = new();
        Parser.Parser parser = new(context, sourceCode, _environment);
        parser.ParseProgram();
    }
}