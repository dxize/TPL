namespace Lexer;

/// <summary>
///  Сканирует текст SQL-запроса, предоставляя три операции: Peek(N), Advance() и IsEnd().
/// </summary>
public class TextScanner(string sql)
{
    private readonly string _sql = sql;
    private int _position;

    /// <summary>
    ///  Читает на N символов вперёд текущей позиции (по умолчанию N=0).
    /// </summary>
    public char Peek(int n = 0)
    {
        int position = _position + n;
        return position >= _sql.Length ? '\0' : _sql[position];
    }

    /// <summary>
    ///  Сдвигает текущую позицию на один символ.
    /// </summary>
    public void Advance()
    {
        _position++;
    }

    public bool IsEnd()
    {
        return _position >= _sql.Length;
    }
}