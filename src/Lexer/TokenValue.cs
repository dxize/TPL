using System;
using System.Globalization;

namespace Lexer;

public class TokenValue
{
    private readonly object _value;

    /// <summary>
    /// Создаёт строковое значение токена.
    /// </summary>
    public TokenValue(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Создаёт целочисленное значение токена.
    /// </summary>
    public TokenValue(int value)
    {
        _value = value;
    }

    /// <summary>
    /// Создаёт числовое значение токена.
    /// </summary>
    public TokenValue(decimal value)
    {
        _value = value;
    }

    /// <summary>
    /// Создаёт логическое значение токена.
    /// </summary>
    public TokenValue(bool value)
    {
        _value = value;
    }

    /// <summary>
    /// Возвращает значение токена в виде строки.
    /// </summary>
    public override string ToString()
    {
        return _value switch
        {
            string s => s,
            int i => i.ToString(CultureInfo.InvariantCulture),
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Возвращает значение как int.
    /// </summary>
    public int ToInt()
    {
        return _value switch
        {
            int i => i,
            string s => int.Parse(s, CultureInfo.InvariantCulture),
            _ => throw new InvalidOperationException("Значение токена не является int.")
        };
    }

    /// <summary>
    /// Возвращает значение как decimal.
    /// </summary>
    public decimal ToDecimal()
    {
        return _value switch
        {
            int i => i,
            decimal d => d,
            string s => decimal.Parse(s, CultureInfo.InvariantCulture),
            _ => throw new InvalidOperationException("Значение токена не является decimal.")
        };
    }

    /// <summary>
    /// Возвращает значение как bool.
    /// </summary>
    public bool ToBool()
    {
        return _value switch
        {
            bool b => b,
            string s => bool.Parse(s),
            _ => throw new InvalidOperationException("Значение токена не является bool.")
        };
    }

    /// <summary>
    /// Сравнивает значения токенов.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is TokenValue other)
        {
            return Equals(_value, other._value);
        }

        return false;
    }

    /// <summary>
    /// Возвращает хеш значения.
    /// </summary>
    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}