using Lexer;

namespace Parser;

public sealed class TokenStream
{
    private readonly Lexer.Lexer _lexer;
    private readonly List<Token> _buffer = [];

    public TokenStream(string sourceCode)
    {
        _lexer = new Lexer.Lexer(sourceCode);
    }

    public Token Peek()
    {
        return Peek(0);
    }

    public Token Peek(int offset)
    {
        while (_buffer.Count <= offset)
        {
            _buffer.Add(_lexer.ParseToken());
        }

        return _buffer[offset];
    }

    public Token Advance()
    {
        Token current = Peek();

        if (_buffer.Count > 0)
        {
            _buffer.RemoveAt(0);
        }

        return current;
    }
}