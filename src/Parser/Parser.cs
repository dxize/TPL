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

        if (token.Type is TokenType.Int or TokenType.Num or TokenType.String)
        {
            AstNode statement = ParseVariableDeclarationStatement();
            Match(TokenType.Semicolon);
            return statement;
        }

        if (token.Type == TokenType.Const)
        {
            AstNode statement = ParseConstantDeclarationStatement();
            Match(TokenType.Semicolon);
            return statement;
        }

        if (token.Type == TokenType.Identifier && _tokens.Peek(1).Type == TokenType.Assign)
        {
            AstNode statement = ParseAssignmentStatement();
            Match(TokenType.Semicolon);
            return statement;
        }

        if (token.Type == TokenType.Print)
        {
            AstNode statement = ParsePrintStatement();
            Match(TokenType.Semicolon);
            return statement;
        }

        if (token.Type == TokenType.Input)
        {
            AstNode statement = ParseInputStatement();
            Match(TokenType.Semicolon);
            return statement;
        }

        if (token.Type == TokenType.Return)
        {
            AstNode statement = ParseReturnStatement();
            Match(TokenType.Semicolon);
            return statement;
        }

        throw new UnexpectedLexemeException("declaration, assignment, input, print or return", token);
    }

    /// <summary>
    /// print_statement = "print", "(", literal_list, ")" ;
    /// literal_list = literal, { ",", literal } ;
    /// </summary>
    private AstNode ParsePrintStatement()
    {
        Match(TokenType.Print);
        Match(TokenType.OpenParenthesis);

        if (_tokens.Peek().Type == TokenType.CloseParenthesis)
        {
            throw new UnexpectedLexemeException("expression", _tokens.Peek());
        }

        List<Expression> arguments = [ParseExpression()];

        while (_tokens.Peek().Type == TokenType.Comma)
        {
            _tokens.Advance();
            arguments.Add(ParseExpression());
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
        Expression value = ParseExpression();
        return new ReturnExpression(value);
    }

    private AstNode ParseInputStatement()
    {
        Match(TokenType.Input);
        Match(TokenType.OpenParenthesis);
        string name = ParseIdentifier();
        Match(TokenType.CloseParenthesis);
        return new InputExpression(name);
    }

    private AstNode ParseVariableDeclarationStatement()
    {
        DataType type = ParseDataType();
        string name = ParseIdentifier();
        Expression? initializer = null;

        if (_tokens.Peek().Type == TokenType.Assign)
        {
            _tokens.Advance();
            initializer = ParseExpression();
        }

        return new VariableDeclarationExpression(type, name, initializer);
    }

    private AstNode ParseConstantDeclarationStatement()
    {
        Match(TokenType.Const);
        DataType type = ParseDataType();
        string name = ParseIdentifier();
        Match(TokenType.Assign);
        Expression initializer = ParseExpression();
        return new ConstantDeclarationExpression(type, name, initializer);
    }

    private AstNode ParseAssignmentStatement()
    {
        string name = ParseIdentifier();
        Match(TokenType.Assign);
        Expression value = ParseExpression();
        return new AssignmentExpression(name, value);
    }

    private Expression ParseExpression()
    {
        return ParseAdditiveExpression();
    }

    private Expression ParseAdditiveExpression()
    {
        Expression expression = ParseMultiplicativeExpression();

        while (_tokens.Peek().Type is TokenType.Plus or TokenType.Minus)
        {
            OperatorKind op = MapBinaryOperator(_tokens.Advance().Type);
            Expression right = ParseMultiplicativeExpression();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    private Expression ParseMultiplicativeExpression()
    {
        Expression expression = ParseUnaryExpression();

        while (_tokens.Peek().Type is TokenType.Multiply or TokenType.Divide or TokenType.IntegerDivide or TokenType.Modulo)
        {
            OperatorKind op = MapBinaryOperator(_tokens.Advance().Type);
            Expression right = ParseUnaryExpression();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    private Expression ParseUnaryExpression()
    {
        if (_tokens.Peek().Type is TokenType.Plus or TokenType.Minus)
        {
            OperatorKind op = MapUnaryOperator(_tokens.Advance().Type);
            Expression operand = ParseUnaryExpression();
            return new UnaryExpression(op, operand);
        }

        return ParsePowerExpression();
    }

    private Expression ParsePowerExpression()
    {
        Expression left = ParsePrimaryExpression();
        if (_tokens.Peek().Type == TokenType.Power)
        {
            OperatorKind op = MapBinaryOperator(_tokens.Advance().Type);
            Expression right = ParsePowerExpression();
            return new BinaryExpression(left, op, right);
        }

        return left;
    }

    private Expression ParsePrimaryExpression()
    {
        Token token = _tokens.Peek();
        switch (token.Type)
        {
            case TokenType.IntegerLiteral:
                return ParseIntLiteral();
            case TokenType.NumLiteral:
                return ParseNumLiteral();
            case TokenType.StringLiteral:
                return ParseStringLiteral();
            case TokenType.Identifier:
            {
                if (_tokens.Peek(1).Type == TokenType.OpenParenthesis)
                {
                    return ParseCallExpression();
                }

                string name = ParseIdentifier();
                return new IdentifierExpression(name);
            }
            case TokenType.OpenParenthesis:
                Match(TokenType.OpenParenthesis);
                Expression nested = ParseExpression();
                Match(TokenType.CloseParenthesis);
                return nested;
            default:
                throw new UnexpectedLexemeException("expression", token);
        }
    }

    private Expression ParseCallExpression()
    {
        string name = ParseIdentifier();
        Match(TokenType.OpenParenthesis);

        List<Expression> arguments = [];
        if (_tokens.Peek().Type != TokenType.CloseParenthesis)
        {
            arguments.Add(ParseExpression());
            while (_tokens.Peek().Type == TokenType.Comma)
            {
                _tokens.Advance();
                arguments.Add(ParseExpression());
            }
        }

        Match(TokenType.CloseParenthesis);
        return new CallExpression(name, arguments);
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

    private DataType ParseDataType()
    {
        Token token = _tokens.Peek();
        return token.Type switch
        {
            TokenType.Int => AdvanceAndGet(DataType.Int),
            TokenType.Num => AdvanceAndGet(DataType.Num),
            TokenType.String => AdvanceAndGet(DataType.String),
            _ => throw new UnexpectedLexemeException("type (int, num, string)", token),
        };
    }

    private DataType AdvanceAndGet(DataType type)
    {
        _tokens.Advance();
        return type;
    }

    private static OperatorKind MapBinaryOperator(TokenType tokenType)
    {
        return tokenType switch
        {
            TokenType.Plus => OperatorKind.Plus,
            TokenType.Minus => OperatorKind.Minus,
            TokenType.Multiply => OperatorKind.Multiply,
            TokenType.Divide => OperatorKind.Divide,
            TokenType.IntegerDivide => OperatorKind.IntegerDivide,
            TokenType.Modulo => OperatorKind.Modulo,
            TokenType.Power => OperatorKind.Power,
            _ => throw new InvalidOperationException($"Unsupported binary operator token: {tokenType}"),
        };
    }

    private static OperatorKind MapUnaryOperator(TokenType tokenType)
    {
        return tokenType switch
        {
            TokenType.Plus => OperatorKind.Plus,
            TokenType.Minus => OperatorKind.Minus,
            _ => throw new InvalidOperationException($"Unsupported unary operator token: {tokenType}"),
        };
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