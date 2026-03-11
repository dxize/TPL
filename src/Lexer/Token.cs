using System.Text;

namespace Lexer;

public class Token(
    TokenType type,
    TokenValue? value = null
)
{
    /// <summary>
    /// Тип токена.
    /// </summary>
    public TokenType Type { get; } = type;

    /// <summary>
    /// Значение токена, если оно есть.
    /// </summary>
    public TokenValue? Value { get; } = value;

    /// <summary>
    /// Сравнивает токены по типу и значению.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is Token other)
        {
            return Type == other.Type && Equals(Value, other.Value);
        }

        return false;
    }

    /// <summary>
    /// Возвращает хеш токена.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine((int)Type, Value);
    }

    /// <summary>
    /// Форматирует токен в виде "Type (Value)".
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Type);

        if (Value != null)
        {
            sb.Append($" ({Value})");
        }

        return sb.ToString();
    }
}