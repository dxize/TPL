namespace Lexer.UnitTests;

public class LexerTest
{
    [Theory]
    [MemberData(nameof(GetTokenizeData))]
    public void Can_tokenize_DEA(string source, List<Token> expected)
    {
        List<Token> actual = Tokenize(source);
        Assert.Equal(expected, actual);
    }

    public static TheoryData<string, List<Token>> GetTokenizeData()
    {
        return new TheoryData<string, List<Token>>
        {
            {
                "const func proc return if else while for to downto break continue true false input print int num string bool",
                [
                    new Token(TokenType.Const), new Token(TokenType.Func),
                    new Token(TokenType.Proc), new Token(TokenType.Return), new Token(TokenType.If),
                    new Token(TokenType.Else), new Token(TokenType.While), new Token(TokenType.For),
                    new Token(TokenType.To), new Token(TokenType.Downto), new Token(TokenType.Break),
                    new Token(TokenType.Continue),
                    new Token(TokenType.True, new TokenValue(true)),
                    new Token(TokenType.False, new TokenValue(false)),
                    new Token(TokenType.Input), new Token(TokenType.Print),
                    new Token(TokenType.Int), new Token(TokenType.Num),
                    new Token(TokenType.String), new Token(TokenType.Bool),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "FOO Foo foo",
                [
                    new Token(TokenType.Identifier, new TokenValue("FOO")),
                    new Token(TokenType.Identifier, new TokenValue("Foo")),
                    new Token(TokenType.Identifier, new TokenValue("foo")),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "x myVar id1",
                [
                    new Token(TokenType.Identifier, new TokenValue("x")),
                    new Token(TokenType.Identifier, new TokenValue("myVar")),
                    new Token(TokenType.Identifier, new TokenValue("id1")),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "0 123 1.23 0.5",
                [
                    new Token(TokenType.IntegerLiteral, new TokenValue(0)),
                    new Token(TokenType.IntegerLiteral, new TokenValue(123)),
                    new Token(TokenType.NumLiteral, new TokenValue(1.23)),
                    new Token(TokenType.NumLiteral, new TokenValue(0.5)),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "\"hello\" \"\" \"line\\nnew\"",
                [
                    new Token(TokenType.StringLiteral, new TokenValue("hello")),
                    new Token(TokenType.StringLiteral, new TokenValue("")),
                    new Token(TokenType.StringLiteral, new TokenValue("line\nnew")),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "= == != < <= > >= + - * / // % ^ && || !",
                [
                    new Token(TokenType.Assign), new Token(TokenType.Equal), new Token(TokenType.NotEqual),
                    new Token(TokenType.Less), new Token(TokenType.LessOrEqual), new Token(TokenType.Greater),
                    new Token(TokenType.GreaterOrEqual), new Token(TokenType.Plus), new Token(TokenType.Minus),
                    new Token(TokenType.Multiply), new Token(TokenType.Divide), new Token(TokenType.IntegerDivide),
                    new Token(TokenType.Modulo), new Token(TokenType.Power), new Token(TokenType.And),
                    new Token(TokenType.Or), new Token(TokenType.Not),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "; , : ( ) { }",
                [
                    new Token(TokenType.Semicolon), new Token(TokenType.Comma),
                    new Token(TokenType.Error, new TokenValue(":")),
                    new Token(TokenType.OpenParenthesis), new Token(TokenType.CloseParenthesis),
                    new Token(TokenType.OpenBrace), new Token(TokenType.CloseBrace),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "tmp # comment\n x",
                [
                    new Token(TokenType.Identifier, new TokenValue("tmp")),
                    new Token(TokenType.Identifier, new TokenValue("x")),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "tmp /* comment */ x",
                [
                    new Token(TokenType.Identifier, new TokenValue("tmp")),
                    new Token(TokenType.Identifier, new TokenValue("x")),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "\"Escapes: \\\" \\\\ \\n \\t \\r\"",
                [
                    new Token(TokenType.StringLiteral, new TokenValue("Escapes: \" \\ \n \t \r")),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "tmp @ x",
                [
                    new Token(TokenType.Identifier, new TokenValue("tmp")),
                    new Token(TokenType.Error, new TokenValue("@")),
                    new Token(TokenType.Identifier, new TokenValue("x")),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "let x: int = 10;",
                [
                    new Token(TokenType.Identifier, new TokenValue("let")),
                    new Token(TokenType.Identifier, new TokenValue("x")),
                    new Token(TokenType.Error, new TokenValue(":")),
                    new Token(TokenType.Int),
                    new Token(TokenType.Assign),
                    new Token(TokenType.IntegerLiteral, new TokenValue(10)),
                    new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)
                ]
            },
            {
                "const PI: float = 3.14;",
                [
                    new Token(TokenType.Const),
                    new Token(TokenType.Identifier, new TokenValue("PI")),
                    new Token(TokenType.Error, new TokenValue(":")),
                    new Token(TokenType.Identifier, new TokenValue("float")),
                    new Token(TokenType.Assign),
                    new Token(TokenType.NumLiteral, new TokenValue(3.14)),
                    new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)
                ]
            },
            {
                "func sum(a: int, b: int): int { return a + b; }",
                [
                    new Token(TokenType.Func),
                    new Token(TokenType.Identifier, new TokenValue("sum")),
                    new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.Identifier, new TokenValue("a")),
                    new Token(TokenType.Error, new TokenValue(":")),
                    new Token(TokenType.Int),
                    new Token(TokenType.Comma),
                    new Token(TokenType.Identifier, new TokenValue("b")),
                    new Token(TokenType.Error, new TokenValue(":")),
                    new Token(TokenType.Int),
                    new Token(TokenType.CloseParenthesis),
                    new Token(TokenType.Error, new TokenValue(":")),
                    new Token(TokenType.Int),
                    new Token(TokenType.OpenBrace),
                    new Token(TokenType.Return),
                    new Token(TokenType.Identifier, new TokenValue("a")),
                    new Token(TokenType.Plus),
                    new Token(TokenType.Identifier, new TokenValue("b")),
                    new Token(TokenType.Semicolon),
                    new Token(TokenType.CloseBrace),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "if (x > 0) { print(x); } else { print(-x); }",
                [
                    new Token(TokenType.If), new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.Identifier, new TokenValue("x")),
                    new Token(TokenType.Greater),
                    new Token(TokenType.IntegerLiteral, new TokenValue(0)),
                    new Token(TokenType.CloseParenthesis), new Token(TokenType.OpenBrace),
                    new Token(TokenType.Print), new Token(TokenType.OpenParenthesis), new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.CloseParenthesis), new Token(TokenType.Semicolon),
                    new Token(TokenType.CloseBrace), new Token(TokenType.Else), new Token(TokenType.OpenBrace),
                    new Token(TokenType.Print), new Token(TokenType.OpenParenthesis), new Token(TokenType.Minus), new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.CloseParenthesis), new Token(TokenType.Semicolon),
                    new Token(TokenType.CloseBrace),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "for i := 0 to 10 { print(i); }",
                [
                    new Token(TokenType.For),
                    new Token(TokenType.Identifier, new TokenValue("i")),
                    new Token(TokenType.Error, new TokenValue(":")),
                    new Token(TokenType.Assign), // := разбирается как : и =
                    new Token(TokenType.IntegerLiteral, new TokenValue(0)),
                    new Token(TokenType.To),
                    new Token(TokenType.IntegerLiteral, new TokenValue(10)),
                    new Token(TokenType.OpenBrace),
                    new Token(TokenType.Print), new Token(TokenType.OpenParenthesis), new Token(TokenType.Identifier, new TokenValue("i")), new Token(TokenType.CloseParenthesis), new Token(TokenType.Semicolon),
                    new Token(TokenType.CloseBrace),
                    new Token(TokenType.EndOfFile)
                ]
            },
            {
                "x = (-b + (b^2 - 4*a*c)^0.5) / (2*a);",
                [
                    new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Assign),
                    new Token(TokenType.OpenParenthesis), new Token(TokenType.Minus), new Token(TokenType.Identifier, new TokenValue("b")),
                    new Token(TokenType.Plus), new Token(TokenType.OpenParenthesis),
                    new Token(TokenType.Identifier, new TokenValue("b")), new Token(TokenType.Power), new Token(TokenType.IntegerLiteral, new TokenValue(2)),
                    new Token(TokenType.Minus), new Token(TokenType.IntegerLiteral, new TokenValue(4)), new Token(TokenType.Multiply),
                    new Token(TokenType.Identifier, new TokenValue("a")), new Token(TokenType.Multiply), new Token(TokenType.Identifier, new TokenValue("c")),
                    new Token(TokenType.CloseParenthesis), new Token(TokenType.Power), new Token(TokenType.NumLiteral, new TokenValue(0.5)),
                    new Token(TokenType.CloseParenthesis), new Token(TokenType.Divide),
                    new Token(TokenType.OpenParenthesis), new Token(TokenType.IntegerLiteral, new TokenValue(2)), new Token(TokenType.Multiply), new Token(TokenType.Identifier, new TokenValue("a")), new Token(TokenType.CloseParenthesis),
                    new Token(TokenType.Semicolon),
                    new Token(TokenType.EndOfFile)
                ]
            },
            { "x + y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Plus), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x - y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Minus), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x * y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Multiply), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x / y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Divide), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x % y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Modulo), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x = y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Assign), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x == y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Equal), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x != y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.NotEqual), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x < y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Less), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x > y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Greater), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x <= y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.LessOrEqual), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x >= y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.GreaterOrEqual), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x && y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.And), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "x || y;", [ new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Or), new Token(TokenType.Identifier, new TokenValue("y")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
            { "!x;", [ new Token(TokenType.Not), new Token(TokenType.Identifier, new TokenValue("x")), new Token(TokenType.Semicolon), new Token(TokenType.EndOfFile)] },
        };
    }

    private List<Token> Tokenize(string source)
    {
        List<Token> results = [];
        Lexer lexer = new(source);

        for (Token t = lexer.ParseToken(); t.Type != TokenType.EndOfFile; t = lexer.ParseToken())
        {
            results.Add(t);
        }

        results.Add(new Token(TokenType.EndOfFile));

        return results;
    }
}