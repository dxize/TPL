using Lexer;

namespace Parser;

public class UnexpectedLexemeException : Exception
{
    public UnexpectedLexemeException()
    {
    }

    public UnexpectedLexemeException(string message)
        : base(message)
    {
    }

    public UnexpectedLexemeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public UnexpectedLexemeException(TokenType expected, Token actual)
        : base($"Expected {expected}, but got {actual.Type}")
    {
    }

    public UnexpectedLexemeException(string expected, Token actual)
        : base($"Expected {expected}, but got {actual.Type}")
    {
    }
}