using DeaLexer = global::Lexer.Lexer;

namespace Lexer.UnitTests;

public class LexerTest
{
    [Theory]
    [MemberData(nameof(GetTokenizeData))]
    public void Can_tokenize_source(string source, List<Token> expected)
    {
        List<Token> actual = Tokenize(source);
        Assert.Equal(expected, actual);
    }

    public static TheoryData<string, List<Token>> GetTokenizeData()
    {
        return new TheoryData<string, List<Token>>
        {
            {
                "const func proc return if else while for to downto break continue true false input print int num string bool main",
                [
                    new Token(TokenType.Const),
                    new Token(TokenType.Func),
                    new Token(TokenType.Proc),
                    new Token(TokenType.Return),
                    new Token(TokenType.If),
                    new Token(TokenType.Else),
                    new Token(TokenType.While),
                    new Token(TokenType.For),
                    new Token(TokenType.To),
                    new Token(TokenType.Downto),
                    new Token(TokenType.Break),
                    new Token(TokenType.Continue),
                    new Token(TokenType.True, new TokenValue(true)),
                    new Token(TokenType.False, new TokenValue(false)),
                    new Token(TokenType.Input),
                    new Token(TokenType.Print),
                    new Token(TokenType.Int),
                    new Token(TokenType.Num),
                    new Token(TokenType.String),
                    new Token(TokenType.Bool),
                    new Token(TokenType.Identifier, new TokenValue("main")),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "fUNc INT Main",
                [
                    new Token(TokenType.Func),
                    new Token(TokenType.Int),
                    new Token(TokenType.Identifier, new TokenValue("Main")),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "var1 abc123 myVar myvar",
                [
                    new Token(TokenType.Identifier, new TokenValue("var1")),
                    new Token(TokenType.Identifier, new TokenValue("abc123")),
                    new Token(TokenType.Identifier, new TokenValue("myVar")),
                    new Token(TokenType.Identifier, new TokenValue("myvar")),
                    new Token(TokenType.EndOfFile),
                ]
            },

            // Литералы
            {
                "123 45.67",
                [
                    new Token(TokenType.IntegerLiteral, new TokenValue(123)),
                    new Token(TokenType.NumLiteral, new TokenValue(45.67)),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "0.5",
                [
                    new Token(TokenType.NumLiteral, new TokenValue(0.5)),
                    new Token(TokenType.EndOfFile),
                ]
            },

            // Строки
            {
                "\"dea\"",
                [
                    new Token(TokenType.StringLiteral, new TokenValue("dea")),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "\"\"",
                [
                    new Token(TokenType.StringLiteral, new TokenValue("")),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "\"quote: \\\" and slash: \\\\ and tab:\\t\"",
                [
                    new Token(TokenType.StringLiteral, new TokenValue("quote: \" and slash: \\ and tab:\t")),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "\"line1\\nline2\\r\"",
                [
                    new Token(TokenType.StringLiteral, new TokenValue("line1\nline2\r")),
                    new Token(TokenType.EndOfFile),
                ]
            },
            {
                "\"# not comment\" \"/* not comment */\"",
                [
                    new Token(TokenType.StringLiteral, new TokenValue("# not comment")),
                    new Token(TokenType.StringLiteral, new TokenValue("/* not comment */")),
                    new Token(TokenType.EndOfFile),
                ]
            },

            // Операторы
            {
                "x = y;",
                [
                    new Token(TokenType.Identifier, new TokenValue("x")),
                    new Token(TokenType.Assign),
                    new Token(TokenType.Identifier, new TokenValue("y")),
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
            {
                "a ^ b;",
                [
                    new Token(TokenType.Identifier, new TokenValue("a")),
                    new Token(TokenType.Power),
                    new Token(TokenType.Identifier, new TokenValue("b")),
                    new Token(TokenType.Semicolon),
                    new Token(TokenType.EndOfFile),
                ]
            },

            // Прочие лексемы (Delimiters)
            {
                "func int main() { print(1, 0.5, \"ok\"); return 0; }",
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
                    new Token(TokenType.NumLiteral, new TokenValue(0.5)),
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

            // Комментарии
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
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidTokenizeData))]
    public void Returns_error_tokens_for_invalid_lexemes(string source, string expectedErrorPayload)
    {
        List<Token> actual = Tokenize(source);
        Assert.Equal(
            [
                new Token(TokenType.Error, new TokenValue(expectedErrorPayload)),
                new Token(TokenType.EndOfFile),
            ],
            actual);
    }

    public static TheoryData<string, string> GetInvalidTokenizeData()
    {
        return new TheoryData<string, string>
        {
            { "/* comment", "Unterminated multi-line comment" },
            { "@", "@" },
            { "\"unterminated", "unterminated" },
            { "01", "01" },
            { "1.", "1." },
            { "012.34", "012.34" },
            { "&", "&" },
            { "|", "|" },
            { "\"\\", "" },
            { "\"\\q", "" },
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