using System.Globalization;

namespace Lexer;

/// <summary>
///  Лексический анализатор языка DEA.
/// </summary>
public class Lexer
{
    private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { "var", TokenType.Var },
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
        { "print", TokenType.Print }
    };

    private readonly TextScanner _scanner;

    public Lexer(string source)
    {
        _scanner = new TextScanner(source);
    }

    /// <summary>
    ///  Распознаёт следующий токен.
    ///  Дополнительные правила:
    ///   1) Если ввод закончился, то возвращаем токен EndOfFile
    ///   2) Пробельные символы пропускаются
    /// </summary>
    public Token ParseToken()
    {
        SkipWhiteSpacesAndComments();

        if (_scanner.IsEnd())
        {
            return new Token(TokenType.EndOfFile);
        }

        char c = _scanner.Peek();

        // Идентификаторы и ключевые слова
        if (char.IsLetter(c))
        {
            return ParseIdentifierOrKeyword();
        }

        // Числовые литералы
        if (char.IsAsciiDigit(c))
        {
            return ParseNumericLiteral();
        }

        // Строковые литералы
        if (c == '"')
        {
            return ParseStringLiteral();
        }

        // Операторы и разделители
        switch (c)
        {
            case ';':
                _scanner.Advance();
                return new Token(TokenType.Semicolon);
            case ',':
                _scanner.Advance();
                return new Token(TokenType.Comma);
            case ':':
                _scanner.Advance();
                return new Token(TokenType.Colon);
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
            
            // Операторы
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
                break;
            case '|':
                if (_scanner.Peek(1) == '|')
                {
                    _scanner.Advance();
                    _scanner.Advance();
                    return new Token(TokenType.Or);
                }
                break;
        }

        _scanner.Advance();
        return new Token(TokenType.Error, new TokenValue(c.ToString()));
    }

    /// <summary>
    ///  Распознаёт идентификаторы и ключевые слова.
    ///  Идентификатор может начинаться с буквы латинского алфавита.
    ///  Содержать буквы и цифры.
    /// </summary>
    private Token ParseIdentifierOrKeyword()
    {
        string value = "";
        
        while (char.IsLetterOrDigit(_scanner.Peek()))
        {
            value += _scanner.Peek();
            _scanner.Advance();
        }

        // Проверяем на совпадение с ключевым словом
        if (Keywords.TryGetValue(value, out TokenType type))
        {
            return new Token(type);
        }

        // Возвращаем токен идентификатора
        return new Token(TokenType.Identifier, new TokenValue(value));
    }

    /// <summary>
    ///  Распознаёт литерал числа.
    ///  Поддерживает целые и десятичные дроби.
    /// </summary>
    private Token ParseNumericLiteral()
    {
        string numberStr = "";
        
        // Считываем целую часть
        while (char.IsAsciiDigit(_scanner.Peek()))
        {
            numberStr += _scanner.Peek();
            _scanner.Advance();
        }

        // Проверяем наличие дробной части
        if (_scanner.Peek() == '.')
        {
            numberStr += '.';
            _scanner.Advance();

            // Считываем дробную часть
            while (char.IsAsciiDigit(_scanner.Peek()))
            {
                numberStr += _scanner.Peek();
                _scanner.Advance();
            }
            
            if (decimal.TryParse(numberStr, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal floatVal))
            {
                 return new Token(TokenType.FloatLiteral, new TokenValue(floatVal));
            }
        }
        else
        {
            if (decimal.TryParse(numberStr, out decimal intVal))
            {
                return new Token(TokenType.IntegerLiteral, new TokenValue(intVal));
            }
        }

        return new Token(TokenType.Error, new TokenValue(numberStr));
    }

    /// <summary>
    ///  Распознаёт строковый литерал в двойных кавычках.
    /// </summary>
    private Token ParseStringLiteral()
    {
        _scanner.Advance(); // Пропускаем открывающую кавычку

        string contents = "";
        while (_scanner.Peek() != '"')
        {
            if (_scanner.IsEnd())
            {
                return new Token(TokenType.Error, new TokenValue(contents));
            }

            if (_scanner.Peek() == '\\')
            {
                if (TryParseEscapeSequence(out char unescaped))
                {
                    contents += unescaped;
                }
                else
                {
                    // Если escape-последовательность не распознана, оставляем как есть или ошибка
                    contents += _scanner.Peek();
                    _scanner.Advance();
                }
            }
            else
            {
                contents += _scanner.Peek();
                _scanner.Advance();
            }
        }

        _scanner.Advance(); // Пропускаем закрывающую кавычку
        return new Token(TokenType.StringLiteral, new TokenValue(contents));
    }

    private bool TryParseEscapeSequence(out char unescaped)
    {
        _scanner.Advance(); // Пропускаем \
        char next = _scanner.Peek();
        
        switch (next)
        {
            case '"': unescaped = '"'; _scanner.Advance(); return true;
            case '\\': unescaped = '\\'; _scanner.Advance(); return true;
            case 'n': unescaped = '\n'; _scanner.Advance(); return true;
            case 't': unescaped = '\t'; _scanner.Advance(); return true;
            case 'r': unescaped = '\r'; _scanner.Advance(); return true;
            default: 
                unescaped = '\0'; 
                return false;
        }
    }

    /// <summary>
    ///  Пропускает пробелы и комментарии.
    /// </summary>
    private void SkipWhiteSpacesAndComments()
    {
        while (true)
        {
            SkipWhiteSpaces();
            
            if (TryParseSingleLineComment()) continue;
            if (TryParseMultiLineComment()) continue;
            
            break;
        }
    }

    private void SkipWhiteSpaces()
    {
        while (char.IsWhiteSpace(_scanner.Peek()))
        {
            _scanner.Advance();
        }
    }

    /// <summary>
    ///  Однострочный комментарий начинается с #
    /// </summary>
    private bool TryParseSingleLineComment()
    {
        if (_scanner.Peek() == '#')
        {
            while (_scanner.Peek() != '\n' && _scanner.Peek() != '\r' && !_scanner.IsEnd())
            {
                _scanner.Advance();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    ///  Многострочный комментарий /* ... */
    /// </summary>
    private bool TryParseMultiLineComment()
    {
        if (_scanner.Peek() == '/' && _scanner.Peek(1) == '*')
        {
            _scanner.Advance();
            _scanner.Advance();

            while (!(_scanner.Peek() == '*' && _scanner.Peek(1) == '/') && !_scanner.IsEnd())
            {
                _scanner.Advance();
            }

            if (!_scanner.IsEnd())
            {
                _scanner.Advance(); // *
                _scanner.Advance(); // /
            }
            return true;
        }
        return false;
    }
}