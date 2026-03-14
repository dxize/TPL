using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Lexer;

namespace Parser;

public sealed class Parser
{
    private readonly TokenStream _tokens;

    public Parser(string code)
    {
        _tokens = new TokenStream(code);
    }

    /// <summary>
    /// Итерация №2:
    /// program = main_function_declaration, end_of_file ;
    /// </summary>
    public ProgramNode ParseProgram()
    {
        FunctionDeclaration mainFunction = ParseMainFunctionDeclaration();
        Match(TokenType.EndOfFile);
        return new ProgramNode(mainFunction);
    }

    /// <summary>
    /// main_function_declaration =
    ///     "func", "int", "main", "(", ")", block ;
    /// </summary>
    private FunctionDeclaration ParseMainFunctionDeclaration()
    {
        Match(TokenType.Func);
        Match(TokenType.Int);

        string name = ParseIdentifier();
        if (!string.Equals(name, "main", StringComparison.Ordinal))
        {
            throw new UnexpectedLexemeException("main", _tokens.Peek());
        }

        Match(TokenType.OpenParenthesis);
        Match(TokenType.CloseParenthesis);

        List<AstNode> body = ParseBlock();

        return new FunctionDeclaration(DataType.Int, name, body);
    }

    /// <summary>
    /// block = "{", { statement }, "}" ;
    /// </summary>
    private List<AstNode> ParseBlock()
    {
        Match(TokenType.OpenBrace);

        List<AstNode> statements = [];

        while (_tokens.Peek().Type != TokenType.CloseBrace)
        {
            statements.Add(ParseStatement());
        }

        Match(TokenType.CloseBrace);
        return statements;
    }

    /// <summary>
    /// Для 2-й итерации:
    /// statement = print_statement, ";" | return_statement, ";" ;
    /// </summary>
    private AstNode ParseStatement()
    {
        Token token = _tokens.Peek();

        if (token.Type == TokenType.Print)
        {
            AstNode statement = ParsePrintStatement();
            Match(TokenType.Semicolon);
            return statement;
        }

        if (token.Type == TokenType.Return)
        {
            AstNode statement = ParseReturnStatement();
            Match(TokenType.Semicolon);
            return statement;
        }

        throw new UnexpectedLexemeException("print or return", token);
    }

    /// <summary>
    /// print_statement = "print", "(", [ literal_list ], ")" ;
    /// literal_list = literal, { ",", literal } ;
    /// </summary>
    private AstNode ParsePrintStatement()
    {
        Match(TokenType.Print);
        Match(TokenType.OpenParenthesis);

        if (_tokens.Peek().Type == TokenType.CloseParenthesis)
        {
            throw new UnexpectedLexemeException("literal", _tokens.Peek());
        }

        List<LiteralExpression> arguments = [ParseLiteral()];

        while (_tokens.Peek().Type == TokenType.Comma)
        {
            _tokens.Advance();
            arguments.Add(ParseLiteral());
        }

        Match(TokenType.CloseParenthesis);

        return new PrintExpression(arguments);
    }

    /// <summary>
    /// Для 2-й итерации return из main ограничиваем int-литералом.
    /// return_statement = "return", int_literal ;
    /// </summary>
    private AstNode ParseReturnStatement()
    {
        Match(TokenType.Return);
        LiteralExpression value = ParseIntLiteral();
        return new ReturnExpression(value);
    }

    private LiteralExpression ParseLiteral()
    {
        Token token = _tokens.Peek();

        return token.Type switch
        {
            TokenType.IntegerLiteral => ParseIntLiteral(),
            TokenType.NumLiteral => ParseNumLiteral(),
            TokenType.StringLiteral => ParseStringLiteral(),
            _ => throw new UnexpectedLexemeException("literal", token),
        };
    }

    private LiteralExpression ParseIntLiteral()
    {
        Token token = _tokens.Peek();

        if (token.Type != TokenType.IntegerLiteral)
        {
            throw new UnexpectedLexemeException(TokenType.IntegerLiteral, token);
        }

        _tokens.Advance();
        return new LiteralExpression(DataType.Int, token.Value!.ToInt());
    }

    private LiteralExpression ParseNumLiteral()
    {
        Token token = _tokens.Peek();

        if (token.Type != TokenType.NumLiteral)
        {
            throw new UnexpectedLexemeException(TokenType.NumLiteral, token);
        }

        _tokens.Advance();
        return new LiteralExpression(DataType.Num, token.Value!.ToDouble());
    }

    private LiteralExpression ParseStringLiteral()
    {
        Token token = _tokens.Peek();

        if (token.Type != TokenType.StringLiteral)
        {
            throw new UnexpectedLexemeException(TokenType.StringLiteral, token);
        }

        _tokens.Advance();
        return new LiteralExpression(DataType.String, token.Value!.ToString()!);
    }

    private string ParseIdentifier()
    {
        Token token = _tokens.Peek();

        if (token.Type != TokenType.Identifier)
        {
            throw new UnexpectedLexemeException(TokenType.Identifier, token);
        }

        _tokens.Advance();
        return token.Value!.ToString();
    }

    private void Match(TokenType expected)
    {
        Token token = _tokens.Peek();

        if (token.Type != expected)
        {
            throw new UnexpectedLexemeException(expected, token);
        }

        _tokens.Advance();
    }
}