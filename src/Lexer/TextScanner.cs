namespace Lexer;

/// <summary>
/// Сканирует исходный текст посимвольно.
/// </summary>
public class TextScanner(string source)
{
    private readonly string _source = source ?? string.Empty;
    private int _position;

    /// <summary>
    /// Читает символ на N позиций вперёд.
    /// </summary>
    public char Peek(int n = 0)
    {
        int position = _position + n;
        return position >= _source.Length ? '\0' : _source[position];
    }

    /// <summary>
    /// Сдвигает текущую позицию на один символ.
    /// </summary>
    public void Advance()
    {
        if (_position < _source.Length)
        {
            _position++;
        }
    }

    /// <summary>
    /// Проверяет конец текста.
    /// </summary>
    public bool IsEnd()
    {
        return _position >= _source.Length;
    }
}