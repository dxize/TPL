using DeaLexer = global::Lexer.Lexer;

namespace Lexer.UnitTests;

public class LexerTest
{
    [Theory]
    [MemberData(nameof(GetTokenizeData))]
    public void Can_tokenize_dea_iteration2_source(string source, List<Token> expected)
    {
        List<Token> actual = Tokenize(source);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Returns_error_for_unterminated_multiline_comment()
    {
        List<Token> actual = Tokenize("/* comment");

        Assert.Equal(
            [
                new Token(TokenType.Error, new TokenValue("Незакрытый многострочный комментарий")),
                new Token(TokenType.EndOfFile),
            ],
            actual);
    }

    [Fact]
    public void Returns_error_for_unknown_symbol()
    {
        List<Token> actual = Tokenize("@");

        Assert.Equal(
            [
                new Token(TokenType.Error, new TokenValue("@")),
                new Token(TokenType.EndOfFile),
            ],
            actual);
    }

    [Fact]
    public void Rejects_integer_with_leading_zero()
    {
        List<Token> actual = Tokenize("012");

        Assert.Equal(
            [
                new Token(TokenType.Error, new TokenValue("012")),
                new Token(TokenType.EndOfFile),
            ],
            actual);
    }

    [Fact]
    public void Accepts_zero_and_zero_point_five()
    {
        List<Token> actual = Tokenize("0 0.5");
        Assert.Equal(
            [
                new Token(TokenType.IntegerLiteral, new TokenValue(0)),
                new Token(TokenType.NumLiteral, new TokenValue(0.5)),
                new Token(TokenType.EndOfFile),
            ],
            actual);
    }

    public static TheoryData<string, List<Token>> GetTokenizeData()
    {
        return new TheoryData<string, List<Token>>
        {
            {
                "func return input print int num string main",
                [
                    new Token(TokenType.Func),
                    new Token(TokenType.Return),
                    new Token(TokenType.Input),
                    new Token(TokenType.Print),
                    new Token(TokenType.Int),
                    new Token(TokenType.Num),
                    new Token(TokenType.String),
                    new Token(TokenType.Identifier, new TokenValue("main")),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "func INT Main",
                [
                    new Token(TokenType.Func),
                    new Token(TokenType.Int),
                    new Token(TokenType.Identifier, new TokenValue("Main")),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "123 45.67 \"dea\"",
                [
                    new Token(TokenType.IntegerLiteral, new TokenValue(123)),
                    new Token(TokenType.NumLiteral, new TokenValue(45.67)),
                    new Token(TokenType.StringLiteral, new TokenValue("dea")),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "func int main() { print(1, 2.5, \"ok\"); return 0; }",
                [
                    new Token(TokenType.Func),
                    new Token(TokenType.Int),
                    new Token(TokenType.Identifier, new TokenValue("main")),
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.CloseParenthesis),
                    new Token(TokenType.OpenBrace),
                    new Token(TokenType.Print),
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.IntegerLiteral, new TokenValue(1)),
                    new Token(TokenType.Comma),
                    new Token(TokenType.NumLiteral, new TokenValue(2.5)),
                    new Token(TokenType.Comma),
                    new Token(TokenType.StringLiteral, new TokenValue("ok")),
                    new Token(TokenType.CloseParenthesis),
                    new Token(TokenType.Semicolon),
                    new Token(TokenType.Return),
                    new Token(TokenType.IntegerLiteral, new TokenValue(0)),
                    new Token(TokenType.Semicolon),
                    new Token(TokenType.CloseBrace),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "# one line\nprint(1); /* two\nline */ return 0;",
                [
                    new Token(TokenType.Print),
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.IntegerLiteral, new TokenValue(1)),
                    new Token(TokenType.CloseParenthesis),
                    new Token(TokenType.Semicolon),
                    new Token(TokenType.Return),
                    new Token(TokenType.IntegerLiteral, new TokenValue(0)),
                    new Token(TokenType.Semicolon),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "x + y - z * 2 / 3 // 4 % 5 == 6 != 7 < 8 <= 9 > 10 >= 11 && a || !b;",
                [
                    new Token(TokenType.Identifier, new TokenValue("x")),
                    new Token(TokenType.Plus),
                    new Token(TokenType.Identifier, new TokenValue("y")),
                    new Token(TokenType.Minus),
                    new Token(TokenType.Identifier, new TokenValue("z")),
                    new Token(TokenType.Multiply),
                    new Token(TokenType.IntegerLiteral, new TokenValue(2)),
                    new Token(TokenType.Divide),
                    new Token(TokenType.IntegerLiteral, new TokenValue(3)),
                    new Token(TokenType.IntegerDivide),
                    new Token(TokenType.IntegerLiteral, new TokenValue(4)),
                    new Token(TokenType.Modulo),
                    new Token(TokenType.IntegerLiteral, new TokenValue(5)),
                    new Token(TokenType.Equal),
                    new Token(TokenType.IntegerLiteral, new TokenValue(6)),
                    new Token(TokenType.NotEqual),
                    new Token(TokenType.IntegerLiteral, new TokenValue(7)),
                    new Token(TokenType.Less),
                    new Token(TokenType.IntegerLiteral, new TokenValue(8)),
                    new Token(TokenType.LessOrEqual),
                    new Token(TokenType.IntegerLiteral, new TokenValue(9)),
                    new Token(TokenType.Greater),
                    new Token(TokenType.IntegerLiteral, new TokenValue(10)),
                    new Token(TokenType.GreaterOrEqual),
                    new Token(TokenType.IntegerLiteral, new TokenValue(11)),
                    new Token(TokenType.And),
                    new Token(TokenType.Identifier, new TokenValue("a")),
                    new Token(TokenType.Or),
                    new Token(TokenType.Not),
                    new Token(TokenType.Identifier, new TokenValue("b")),
                    new Token(TokenType.Semicolon),
                    new Token(TokenType.EndOfFile),
                ]
            },
        };
    }

    private static List<Token> Tokenize(string source)
    {
        List<Token> results = [];
        DeaLexer lexer = new(source);

        for (Token t = lexer.ParseToken(); t.Type != TokenType.EndOfFile; t = lexer.ParseToken())
        {
            results.Add(t);
        }

        results.Add(new Token(TokenType.EndOfFile));
        return results;
    }
}