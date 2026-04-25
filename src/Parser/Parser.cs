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
    /// program = { global_decl | func_decl | proc_decl }, main_func_decl, EOF ;
    /// </summary>
    public ProgramNode ParseProgram()
    {
        List<Declaration> globalDeclarations = [];
        List<FunctionDeclaration> userFunctions = [];

        while (!IsMainFunctionAhead())
        {
            Token token = _tokens.Peek();

            if (token.Type == TokenType.Func)
            {
                userFunctions.Add(ParseFunctionDeclaration());
                continue;
            }

            if (token.Type == TokenType.Proc)
            {
                userFunctions.Add(ParseProcedureDeclaration());
                continue;
            }

            if (token.Type == TokenType.Const)
            {
                globalDeclarations.Add(ParseConstantDeclaration());
                Match(TokenType.Semicolon);
                continue;
            }

            if (token.Type is TokenType.Int or TokenType.Num or TokenType.String or TokenType.Bool)
            {
                globalDeclarations.Add(ParseVariableDeclaration());
                Match(TokenType.Semicolon);
                continue;
            }

            throw new UnexpectedLexemeException("top-level declaration or func int main()", token);
        }

        FunctionDeclaration mainFunction = ParseMainFunctionDeclaration();
        Match(TokenType.EndOfFile);
        return new ProgramNode(globalDeclarations, userFunctions, mainFunction);
    }

    /// <summary>
    /// Проверяет, начинается ли func int main().
    /// </summary>
    private bool IsMainFunctionAhead()
    {
        return _tokens.Peek().Type == TokenType.Func
            && _tokens.Peek(1).Type == TokenType.Int
            && _tokens.Peek(2).Type == TokenType.Identifier
            && string.Equals(_tokens.Peek(2).Value?.ToString(), "main", StringComparison.Ordinal)
            && _tokens.Peek(3).Type == TokenType.OpenParenthesis;
    }

    /// <summary>
    /// variable_declaration = type, identifier, [ "=", expression ] ;
    /// </summary>
    private Declaration ParseVariableDeclaration()
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

    /// <summary>
    /// constant_declaration = "const", type, identifier, "=", expression ;
    /// </summary>
    private Declaration ParseConstantDeclaration()
    {
        Match(TokenType.Const);
        DataType type = ParseDataType();
        string name = ParseIdentifier();
        Match(TokenType.Assign);
        Expression initializer = ParseExpression();
        return new ConstantDeclarationExpression(type, name, initializer);
    }

    /// <summary>
    /// main_func_decl = "func", "int", "main", "(", ")", block ;
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
        return new FunctionDeclaration(DataType.Int, name, [], ParseBlock());
    }

    /// <summary>
    /// func_decl = "func", type, identifier, "(", [ params ], ")", block ;
    /// </summary>
    private FunctionDeclaration ParseFunctionDeclaration()
    {
        Match(TokenType.Func);
        DataType returnType = ParseDataType();
        string name = ParseIdentifier();
        Match(TokenType.OpenParenthesis);
        List<ParameterDeclaration> parameters = ParseOptionalParameterList();
        Match(TokenType.CloseParenthesis);
        return new FunctionDeclaration(returnType, name, parameters, ParseBlock());
    }

    /// <summary>
    /// proc_decl = "proc", identifier, "(", [ params ], ")", block ;
    /// </summary>
    private FunctionDeclaration ParseProcedureDeclaration()
    {
        Match(TokenType.Proc);
        string name = ParseIdentifier();
        Match(TokenType.OpenParenthesis);
        List<ParameterDeclaration> parameters = ParseOptionalParameterList();
        Match(TokenType.CloseParenthesis);
        return new FunctionDeclaration(DataType.Void, name, parameters, ParseBlock());
    }

    /// <summary>
    /// params = parameter, { ",", parameter } ;
    /// </summary>
    private List<ParameterDeclaration> ParseOptionalParameterList()
    {
        List<ParameterDeclaration> parameters = [];

        if (_tokens.Peek().Type == TokenType.CloseParenthesis)
        {
            return parameters;
        }

        parameters.Add(ParseParameter());
        while (_tokens.Peek().Type == TokenType.Comma)
        {
            _tokens.Advance();
            parameters.Add(ParseParameter());
        }

        return parameters;
    }

    /// <summary>
    /// parameter = type, identifier ;
    /// </summary>
    private ParameterDeclaration ParseParameter()
    {
        DataType type = ParseDataType();
        string name = ParseIdentifier();
        return new ParameterDeclaration(type, name);
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
    /// statement =
    ///     declaration ";"
    ///   | assignment ";"
    ///   | procedure_call ";"
    ///   | print ";"
    ///   | input ";"
    ///   | return ";"
    ///   | if_statement ;
    /// </summary>
    private AstNode ParseStatement()
    {
        Token token = _tokens.Peek();

        if (token.Type is TokenType.Int or TokenType.Num or TokenType.String or TokenType.Bool)
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

        if (token.Type == TokenType.Identifier && _tokens.Peek(1).Type == TokenType.OpenParenthesis)
        {
            AstNode statement = ParseProcedureCallStatement();
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

        if (token.Type == TokenType.If)
        {
            return ParseIfStatement();
        }

        if (token.Type == TokenType.While)
        {
            return ParseWhileStatement();
        }

        if (token.Type == TokenType.For)
        {
            return ParseForStatement();
        }

        if (token.Type == TokenType.Break)
        {
            Match(TokenType.Break);
            if (_tokens.Peek().Type == TokenType.Semicolon)
            {
                _tokens.Advance();
            }

            return new BreakStatement();
        }

        if (token.Type == TokenType.Continue)
        {
            Match(TokenType.Continue);
            if (_tokens.Peek().Type == TokenType.Semicolon)
            {
                _tokens.Advance();
            }

            return new ContinueStatement();
        }

        throw new UnexpectedLexemeException("statement", token);
    }

    /// <summary>
    /// print_statement = "print", "(", expression, { ",", expression }, ")" ;
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
    /// return_statement = "return", [ expression ] ;
    /// </summary>
    private AstNode ParseReturnStatement()
    {
        Match(TokenType.Return);
        Expression? value = _tokens.Peek().Type == TokenType.Semicolon ? null : ParseExpression();
        return new ReturnExpression(value);
    }

    /// <summary>
    /// input_statement = "input", "(", identifier, ")" ;
    /// </summary>
    private AstNode ParseInputStatement()
    {
        Match(TokenType.Input);
        Match(TokenType.OpenParenthesis);
        string name = ParseIdentifier();
        Match(TokenType.CloseParenthesis);
        return new InputExpression(name);
    }

    /// <summary>
    /// if_statement = "if", "(", expression, ")", block, [ "else", block ] ;
    /// </summary>
    private AstNode ParseIfStatement()
    {
        Match(TokenType.If);
        Match(TokenType.OpenParenthesis);
        Expression condition = ParseExpression();
        Match(TokenType.CloseParenthesis);
        List<AstNode> thenBody = ParseBlock();
        List<AstNode>? elseBody = null;

        if (_tokens.Peek().Type == TokenType.Else)
        {
            _tokens.Advance();
            elseBody = ParseBlock();
        }

        return new IfStatement(condition, thenBody, elseBody);
    }

    /// <summary>
    /// while_statement = "while", "(", expression, ")", block ;
    /// </summary>
    private AstNode ParseWhileStatement()
    {
        Match(TokenType.While);
        Match(TokenType.OpenParenthesis);
        Expression condition = ParseExpression();
        Match(TokenType.CloseParenthesis);
        List<AstNode> body = ParseBlock();
        return new WhileStatement(condition, body);
    }

    /// <summary>
    /// for_statement = "for", "(", identifier, "=", expression, ("to" | "downto"), expression, ")", block ;
    /// </summary>
    private AstNode ParseForStatement()
    {
        Match(TokenType.For);
        Match(TokenType.OpenParenthesis);
        string variableName = ParseIdentifier();
        Match(TokenType.Assign);
        Expression start = ParseExpression();
        bool descending = false;

        if (_tokens.Peek().Type == TokenType.To)
        {
            _tokens.Advance();
        }
        else if (_tokens.Peek().Type == TokenType.Downto)
        {
            _tokens.Advance();
            descending = true;
        }
        else
        {
            throw new UnexpectedLexemeException("'to' or 'downto'", _tokens.Peek());
        }

        Expression end = ParseExpression();
        Match(TokenType.CloseParenthesis);
        List<AstNode> body = ParseBlock();
        return new ForStatement(variableName, start, end, body, descending);
    }

    /// <summary>
    /// break_statement = "break" ;
    /// </summary>
    private AstNode ParseBreakStatement()
    {
        Match(TokenType.Break);
        return new BreakStatement();
    }

    /// <summary>
    /// continue_statement = "continue" ;
    /// </summary>
    private AstNode ParseContinueStatement()
    {
        Match(TokenType.Continue);
        return new ContinueStatement();
    }

    /// <summary>
    /// local_variable_declaration = type, identifier, [ "=", expression ] ;
    /// </summary>
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

    /// <summary>
    /// local_constant_declaration = "const", type, identifier, "=", expression ;
    /// </summary>
    private AstNode ParseConstantDeclarationStatement()
    {
        Match(TokenType.Const);
        DataType type = ParseDataType();
        string name = ParseIdentifier();
        Match(TokenType.Assign);
        Expression initializer = ParseExpression();
        return new ConstantDeclarationExpression(type, name, initializer);
    }

    /// <summary>
    /// assignment = identifier, "=", expression ;
    /// </summary>
    private AstNode ParseAssignmentStatement()
    {
        string name = ParseIdentifier();
        Match(TokenType.Assign);
        Expression value = ParseExpression();
        return new AssignmentExpression(name, value);
    }

    /// <summary>
    /// procedure_call = identifier, "(", [ arguments ], ")" ;
    /// </summary>
    private AstNode ParseProcedureCallStatement()
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
        return new ProcedureCallStatement(name, arguments);
    }

    /// <summary>
    /// expression = logical_or_expression ;
    /// </summary>
    private Expression ParseExpression() => ParseLogicalOrExpression();

    /// <summary>
    /// logical_or_expression = logical_and_expression, { "||", logical_and_expression } ;
    /// </summary>
    private Expression ParseLogicalOrExpression()
    {
        Expression expression = ParseLogicalAndExpression();

        while (_tokens.Peek().Type == TokenType.Or)
        {
            OperatorKind op = MapBinaryOperator(_tokens.Advance().Type);
            Expression right = ParseLogicalAndExpression();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    /// <summary>
    /// logical_and_expression = equality_expression, { "&&", equality_expression } ;
    /// </summary>
    private Expression ParseLogicalAndExpression()
    {
        Expression expression = ParseEqualityExpression();

        while (_tokens.Peek().Type == TokenType.And)
        {
            OperatorKind op = MapBinaryOperator(_tokens.Advance().Type);
            Expression right = ParseEqualityExpression();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    /// <summary>
    /// equality_expression = comparison_expression, { ("==" | "!="), comparison_expression } ;
    /// </summary>
    private Expression ParseEqualityExpression()
    {
        Expression expression = ParseComparisonExpression();

        while (_tokens.Peek().Type is TokenType.Equal or TokenType.NotEqual)
        {
            OperatorKind op = MapBinaryOperator(_tokens.Advance().Type);
            Expression right = ParseComparisonExpression();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    /// <summary>
    /// comparison_expression = additive_expression, { ("<" | "<=" | ">" | ">="), additive_expression } ;
    /// </summary>
    private Expression ParseComparisonExpression()
    {
        Expression expression = ParseAdditiveExpression();

        while (_tokens.Peek().Type is TokenType.Less or TokenType.LessOrEqual or TokenType.Greater or TokenType.GreaterOrEqual)
        {
            OperatorKind op = MapBinaryOperator(_tokens.Advance().Type);
            Expression right = ParseAdditiveExpression();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    /// <summary>
    /// additive_expression = multiplicative_expression, { ("+" | "-"), multiplicative_expression } ;
    /// </summary>
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

    /// <summary>
    /// multiplicative_expression = unary_expression, { ("*" | "/" | "//" | "%"), unary_expression } ;
    /// </summary>
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

    /// <summary>
    /// unary_expression = [ "+" | "-" | "!" ], unary_expression | power_expression ;
    /// </summary>
    private Expression ParseUnaryExpression()
    {
        if (_tokens.Peek().Type is TokenType.Plus or TokenType.Minus or TokenType.Not)
        {
            OperatorKind op = MapUnaryOperator(_tokens.Advance().Type);
            Expression operand = ParseUnaryExpression();
            return new UnaryExpression(op, operand);
        }

        return ParsePowerExpression();
    }

    /// <summary>
    /// power_expression = primary_expression, [ "^", power_expression ] ;
    /// </summary>
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

    /// <summary>
    /// primary_expression =
    ///     int_literal
    ///   | num_literal
    ///   | string_literal
    ///   | bool_literal
    ///   | identifier
    ///   | call_expression
    ///   | "(", expression, ")" ;
    /// </summary>
    private Expression ParsePrimaryExpression()
    {
        Token token = _tokens.Peek();
        return token.Type switch
        {
            TokenType.IntegerLiteral => ParseIntLiteral(),
            TokenType.NumLiteral => ParseNumLiteral(),
            TokenType.StringLiteral => ParseStringLiteral(),
            TokenType.True or TokenType.False => ParseBoolLiteral(),
            TokenType.Identifier => _tokens.Peek(1).Type == TokenType.OpenParenthesis
                ? ParseCallExpression()
                : new IdentifierExpression(ParseIdentifier()),
            TokenType.OpenParenthesis => ParseParenthesizedExpression(),
            _ => throw new UnexpectedLexemeException("expression", token),
        };
    }

    /// <summary>
    /// parenthesized_expression = "(", expression, ")" ;
    /// </summary>
    private Expression ParseParenthesizedExpression()
    {
        Match(TokenType.OpenParenthesis);
        Expression nested = ParseExpression();
        Match(TokenType.CloseParenthesis);
        return nested;
    }

    /// <summary>
    /// call_expression = identifier, "(", [ arguments ], ")" ;
    /// </summary>
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

    /// <summary>
    /// int_literal
    /// </summary>
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

    /// <summary>
    /// num_literal
    /// </summary>
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

    /// <summary>
    /// string_literal
    /// </summary>
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

    /// <summary>
    /// bool_literal = "true" | "false" ;
    /// </summary>
    private LiteralExpression ParseBoolLiteral()
    {
        Token token = _tokens.Peek();
        if (token.Type is not (TokenType.True or TokenType.False))
        {
            throw new UnexpectedLexemeException("bool literal", token);
        }

        _tokens.Advance();
        return new LiteralExpression(DataType.Bool, token.Value!.ToBool());
    }

    /// <summary>
    /// identifier
    /// </summary>
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

    /// <summary>
    /// type = "int" | "num" | "string" | "bool" ;
    /// </summary>
    private DataType ParseDataType()
    {
        Token token = _tokens.Peek();
        return token.Type switch
        {
            TokenType.Int => AdvanceAndGet(DataType.Int),
            TokenType.Num => AdvanceAndGet(DataType.Num),
            TokenType.String => AdvanceAndGet(DataType.String),
            TokenType.Bool => AdvanceAndGet(DataType.Bool),
            _ => throw new UnexpectedLexemeException("type (int, num, string, bool)", token),
        };
    }

    /// <summary>
    /// Сдвигает поток токенов и возвращает тип.
    /// </summary>
    private DataType AdvanceAndGet(DataType type)
    {
        _tokens.Advance();
        return type;
    }

    /// <summary>
    /// Преобразует бинарный токен в OperatorKind.
    /// </summary>
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
            TokenType.Equal => OperatorKind.Equal,
            TokenType.NotEqual => OperatorKind.NotEqual,
            TokenType.Less => OperatorKind.Less,
            TokenType.LessOrEqual => OperatorKind.LessOrEqual,
            TokenType.Greater => OperatorKind.Greater,
            TokenType.GreaterOrEqual => OperatorKind.GreaterOrEqual,
            TokenType.And => OperatorKind.And,
            TokenType.Or => OperatorKind.Or,
            _ => throw new InvalidOperationException($"Unsupported binary operator token: {tokenType}"),
        };
    }

    /// <summary>
    /// Преобразует унарный токен в OperatorKind.
    /// </summary>
    private static OperatorKind MapUnaryOperator(TokenType tokenType)
    {
        return tokenType switch
        {
            TokenType.Plus => OperatorKind.Plus,
            TokenType.Minus => OperatorKind.Minus,
            TokenType.Not => OperatorKind.Not,
            _ => throw new InvalidOperationException($"Unsupported unary operator token: {tokenType}"),
        };
    }

    /// <summary>
    /// Проверяет ожидаемый токен и сдвигает поток.
    /// </summary>
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