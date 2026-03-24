using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Lexer;

/// <summary>
/// Лексер языка DEA + E.
/// </summary>
public class Lexer
{
    private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { "const", TokenType.Const },
        { "func", TokenType.Func },
        { "proc", TokenType.Proc },
        { "return", TokenType.Return },
        { "if", TokenType.If },
        { "else", TokenType.Else },
        { "while", TokenType.While },
        { "for", TokenType.For },
        { "to", TokenType.To },
        { "downto", TokenType.Downto },
        { "break", TokenType.Break },
        { "continue", TokenType.Continue },
        { "true", TokenType.True },
        { "false", TokenType.False },
        { "input", TokenType.Input },
        { "print", TokenType.Print },
        { "int", TokenType.Int },
        { "num", TokenType.Num },
        { "string", TokenType.String },
        { "bool", TokenType.Bool },
    };

    private readonly TextScanner _scanner;
    private bool _hasUnterminatedMultiLineComment;

    /// <summary>
    /// Создаёт лексер.
    /// </summary>
    public Lexer(string source)
    {
        _scanner = new TextScanner(source);
    }

    /// <summary>
    /// Возвращает следующий токен.
    /// </summary>
    public Token ParseToken()
    {
        SkipWhiteSpacesAndComments();

        if (_hasUnterminatedMultiLineComment)
        {
            _hasUnterminatedMultiLineComment = false;
            return new Token(TokenType.Error, new TokenValue("Незакрытый многострочный комментарий"));
        }

        if (_scanner.IsEnd())
        {
            return new Token(TokenType.EndOfFile);
        }

        char c = _scanner.Peek();

        // Идентификатор или ключевое слово
        if (IsLatinLetter(c))
        {
            return ParseIdentifierOrKeyword();
        }

        // Числовой литерал
        if (char.IsAsciiDigit(c))
        {
            return ParseNumericLiteral();
        }

        // Строковый литерал
        if (c == '"')
        {
            return ParseStringLiteral();
        }

        // Разделители и операторы
        switch (c)
        {
            case ';':
                _scanner.Advance();
                return new Token(TokenType.Semicolon);

            case ',':
                _scanner.Advance();
                return new Token(TokenType.Comma);

            case '(':
                _scanner.Advance();
                return new Token(TokenType.OpenParenthesis);

            case ')':
                _scanner.Advance();
                return new Token(TokenType.CloseParenthesis);

            case '{':
                _scanner.Advance();
                return new Token(TokenType.OpenBrace);

            case '}':
                _scanner.Advance();
                return new Token(TokenType.CloseBrace);

            case '+':
                _scanner.Advance();
                return new Token(TokenType.Plus);

            case '-':
                _scanner.Advance();
                return new Token(TokenType.Minus);

            case '*':
                _scanner.Advance();
                return new Token(TokenType.Multiply);

            case '%':
                _scanner.Advance();
                return new Token(TokenType.Modulo);

            case '^':
                _scanner.Advance();
                return new Token(TokenType.Power);

            case '/':
                _scanner.Advance();
                if (_scanner.Peek() == '/')
                {
                    _scanner.Advance();
                    return new Token(TokenType.IntegerDivide);
                }

                return new Token(TokenType.Divide);

            case '=':
                _scanner.Advance();
                if (_scanner.Peek() == '=')
                {
                    _scanner.Advance();
                    return new Token(TokenType.Equal);
                }

                return new Token(TokenType.Assign);

            case '!':
                _scanner.Advance();
                if (_scanner.Peek() == '=')
                {
                    _scanner.Advance();
                    return new Token(TokenType.NotEqual);
                }

                return new Token(TokenType.Not);

            case '<':
                _scanner.Advance();
                if (_scanner.Peek() == '=')
                {
                    _scanner.Advance();
                    return new Token(TokenType.LessOrEqual);
                }

                return new Token(TokenType.Less);

            case '>':
                _scanner.Advance();
                if (_scanner.Peek() == '=')
                {
                    _scanner.Advance();
                    return new Token(TokenType.GreaterOrEqual);
                }

                return new Token(TokenType.Greater);

            case '&':
                if (_scanner.Peek(1) == '&')
                {
                    _scanner.Advance();
                    _scanner.Advance();
                    return new Token(TokenType.And);
                }

                _scanner.Advance();
                return new Token(TokenType.Error, new TokenValue("&"));

            case '|':
                if (_scanner.Peek(1) == '|')
                {
                    _scanner.Advance();
                    _scanner.Advance();
                    return new Token(TokenType.Or);
                }

                _scanner.Advance();
                return new Token(TokenType.Error, new TokenValue("|"));
        }

        _scanner.Advance();
        return new Token(TokenType.Error, new TokenValue(c.ToString()));
    }

    /// <summary>
    /// Читает идентификатор или ключевое слово.
    /// </summary>
    private Token ParseIdentifierOrKeyword()
    {
        StringBuilder sb = new();

        while (IsLatinLetterOrDigit(_scanner.Peek()))
        {
            sb.Append(_scanner.Peek());
            _scanner.Advance();
        }

        string value = sb.ToString();

        if (Keywords.TryGetValue(value, out TokenType type))
        {
            return type switch
            {
                TokenType.True => new Token(TokenType.True, new TokenValue(true)),
                TokenType.False => new Token(TokenType.False, new TokenValue(false)),
                _ => new Token(type),
            };
        }

        return new Token(TokenType.Identifier, new TokenValue(value));
    }

    /// <summary>
    /// Читает целый или num-литерал.
    /// </summary>
    private Token ParseNumericLiteral()
    {
        StringBuilder sb = new();

        while (char.IsAsciiDigit(_scanner.Peek()))
        {
            sb.Append(_scanner.Peek());
            _scanner.Advance();
        }

        // Ведущий 0 запрещён, если дальше идёт цифра
        if (sb.Length > 1 && sb[0] == '0')
        {
            // Дочитываем дробную часть, чтобы ошибка покрыла весь литерал
            if (_scanner.Peek() == '.')
            {
                sb.Append('.');
                _scanner.Advance();

                while (char.IsAsciiDigit(_scanner.Peek()))
                {
                    sb.Append(_scanner.Peek());
                    _scanner.Advance();
                }
            }

            return new Token(TokenType.Error, new TokenValue(sb.ToString()));
        }

        bool hasFraction = false;

        if (_scanner.Peek() == '.')
        {
            hasFraction = true;
            sb.Append('.');
            _scanner.Advance();

            if (!char.IsAsciiDigit(_scanner.Peek()))
            {
                return new Token(TokenType.Error, new TokenValue(sb.ToString()));
            }

            while (char.IsAsciiDigit(_scanner.Peek()))
            {
                sb.Append(_scanner.Peek());
                _scanner.Advance();
            }
        }

        string value = sb.ToString();

        if (hasFraction)
        {
            if (double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double numValue))
            {
                return new Token(TokenType.NumLiteral, new TokenValue(numValue));
            }

            return new Token(TokenType.Error, new TokenValue(value));
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
        {
            return new Token(TokenType.IntegerLiteral, new TokenValue(intValue));
        }

        return new Token(TokenType.Error, new TokenValue(value));
    }

    /// <summary>
    /// Читает строковый литерал.
    /// </summary>
    private Token ParseStringLiteral()
    {
        _scanner.Advance(); // "

        StringBuilder contents = new();

        while (!_scanner.IsEnd())
        {
            char current = _scanner.Peek();

            if (current == '"')
            {
                _scanner.Advance();
                return new Token(TokenType.StringLiteral, new TokenValue(contents.ToString()));
            }

            if (current == '\r' || current == '\n')
            {
                return new Token(TokenType.Error, new TokenValue(contents.ToString()));
            }

            if (current == '\\')
            {
                if (TryParseEscapeSequence(out char unescaped))
                {
                    contents.Append(unescaped);
                    continue;
                }

                return new Token(TokenType.Error, new TokenValue(contents.ToString()));
            }

            contents.Append(current);
            _scanner.Advance();
        }

        return new Token(TokenType.Error, new TokenValue(contents.ToString()));
    }

    /// <summary>
    /// Читает escape-последовательность.
    /// </summary>
    private bool TryParseEscapeSequence(out char unescaped)
    {
        _scanner.Advance(); // \

        if (_scanner.IsEnd())
        {
            unescaped = '\0';
            return false;
        }

        char next = _scanner.Peek();

        switch (next)
        {
            case '"':
                unescaped = '"';
                _scanner.Advance();
                return true;

            case '\\':
                unescaped = '\\';
                _scanner.Advance();
                return true;

            case 'n':
                unescaped = '\n';
                _scanner.Advance();
                return true;

            case 't':
                unescaped = '\t';
                _scanner.Advance();
                return true;

            case 'r':
                unescaped = '\r';
                _scanner.Advance();
                return true;

            default:
                _scanner.Advance();
                unescaped = '\0';
                return false;
        }
    }

    /// <summary>
    /// Пропускает пробелы и комментарии.
    /// </summary>
    private void SkipWhiteSpacesAndComments()
    {
        while (true)
        {
            SkipWhiteSpaces();

            if (TryParseSingleLineComment())
            {
                continue;
            }

            if (TryParseMultiLineComment())
            {
                continue;
            }

            break;
        }
    }

    /// <summary>
    /// Пропускает пробельные символы.
    /// </summary>
    private void SkipWhiteSpaces()
    {
        while (char.IsWhiteSpace(_scanner.Peek()))
        {
            _scanner.Advance();
        }
    }

    /// <summary>
    /// Пропускает однострочный комментарий.
    /// </summary>
    private bool TryParseSingleLineComment()
    {
        if (_scanner.Peek() != '#')
        {
            return false;
        }

        while (_scanner.Peek() != '\n' && _scanner.Peek() != '\r' && !_scanner.IsEnd())
        {
            _scanner.Advance();
        }

        return true;
    }

    /// <summary>
    /// Пропускает многострочный комментарий.
    /// Если комментарий не закрыт, выставляет ошибку.
    /// </summary>
    private bool TryParseMultiLineComment()
    {
        if (!(_scanner.Peek() == '/' && _scanner.Peek(1) == '*'))
        {
            return false;
        }

        _scanner.Advance();
        _scanner.Advance();

        while (!_scanner.IsEnd())
        {
            if (_scanner.Peek() == '*' && _scanner.Peek(1) == '/')
            {
                _scanner.Advance();
                _scanner.Advance();
                return true;
            }

            _scanner.Advance();
        }

        _hasUnterminatedMultiLineComment = true;
        return true;
    }

    /// <summary>
    /// Проверяет латинскую букву.
    /// </summary>
    private static bool IsLatinLetter(char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

    /// <summary>
    /// Проверяет латинскую букву или цифру.
    /// </summary>
    private static bool IsLatinLetterOrDigit(char c)
    {
        return IsLatinLetter(c) || char.IsAsciiDigit(c);
    }
}