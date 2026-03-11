using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Execution;

using Lexer;

namespace Parser;

public class Parser
{
    private readonly TokenStream _tokens;
    private readonly AstEvaluator _evaluator;

    public Parser(Context context, string code, IEnvironment environment)
    {
        _tokens = new TokenStream(code);
        _evaluator = new AstEvaluator(context, environment);
    }

    /// <summary>
    /// Разбирает программу языка DEA.
    /// Правила: program = top_level_statement, { top_level_statement }, end_of_file ;
    /// </summary>
    public void ParseProgram()
    {
        while (_tokens.Peek().Type != TokenType.EndOfFile)
        {
            AstNode node = ParseTopLevelStatement();
            _evaluator.Execute(node);
        }
    }

    /// <summary>
    /// Выполняет разбор выражения и возвращает результат.
    /// </summary>
    public Row EvaluateExpression()
    {
        Expression expression = ParseExpression();
        double result = _evaluator.Evaluate(expression);
        return new Row(result);
    }

    /// <summary>
    /// Разбирает верхнеуровневую инструкцию.
    /// Правила: top_level_statement = function_declaration | statement ;
    /// </summary>
    private AstNode ParseTopLevelStatement()
    {
        Token token = _tokens.Peek();

        if (token.Type == TokenType.Func || token.Type == TokenType.Proc)
        {
            return ParseFunctionDeclaration();
        }

        return ParseStatement();
    }

    /// <summary>
    /// Разбирает объявление функции.
    /// Правила: function_declaration = "func", identifier, "(", [ parameter_list ], ")", "{", { function_statement_item }, "}" ;
    /// </summary>
    private AstNode ParseFunctionDeclaration()
    {
        if (_tokens.Peek().Type == TokenType.Func)
        {
            Match(TokenType.Func);
        }
        else
        {
            Match(TokenType.Proc);
        }
        
        string functionName = ParseIdentifier();
        List<string> parameters = ParseParameterList();
        List<AstNode> body = ParseFunctionBlock();

        return new FunctionDeclaration(functionName, parameters, body);
    }

    /// <summary>
    /// Разбирает список параметров.
    /// Правила: parameter_list = identifier, { ",", identifier } ;
    /// </summary>
    private List<string> ParseParameterList()
    {
        Match(TokenType.OpenParenthesis);
        List<string> parameters = [];

        if (_tokens.Peek().Type != TokenType.CloseParenthesis)
        {
            parameters.Add(ParseIdentifier());

            while (_tokens.Peek().Type == TokenType.Comma)
            {
                _tokens.Advance();
                parameters.Add(ParseIdentifier());
            }
        }

        Match(TokenType.CloseParenthesis);
        return parameters;
    }

    /// <summary>
    /// Разбирает блок функции.
    /// Правила: блок инструкций внутри функций
    /// </summary>
    private List<AstNode> ParseFunctionBlock()
    {
        Match(TokenType.OpenBrace);
        List<AstNode> nodes = [];

        while (_tokens.Peek().Type != TokenType.CloseBrace && _tokens.Peek().Type != TokenType.EndOfFile)
        {
            AstNode node = ParseStatement();
            nodes.Add(node);
        }

        Match(TokenType.CloseBrace);
        return nodes;
    }

    /// <summary>
    /// Разбирает инструкцию верхнего уровня или внутри блока.
    /// Правила: statement = simple_statement, ";" | compound_statement | return_statement ;
    /// Универсальный метод, который обрабатывает все типы инструкций, включая return.
    /// </summary>
    private AstNode ParseStatement()
    {
        Token token = _tokens.Peek();

        if (token.Type == TokenType.Const)
        {
            AstNode statement = ParseConstantDeclaration();
            Match(TokenType.Semicolon);
            return statement;
        }

        if (token.Type == TokenType.Var)
        {
            AstNode statement = ParseVariableDeclaration();
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
            AstNode statement = ParseBreakStatement();
            Match(TokenType.Semicolon);
            return statement;
        }

        if (token.Type == TokenType.Continue)
        {
            AstNode statement = ParseContinueStatement();
            Match(TokenType.Semicolon);
            return statement;
        }

        if (token.Type == TokenType.Identifier)
        {
            return ParseAssignmentOrFunctionCall();
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
            return ParseReturnStatement();
        }

        throw new UnexpectedLexemeException("statement", token);
    }

    /// <summary>
    /// Разбирает объявление переменной.
    /// Правила: variable_declaration = "var", identifier, [ "=", expression ] ;
    /// </summary>
    private AstNode ParseVariableDeclaration()
    {
        Match(TokenType.Var);
        string name = ParseIdentifier();
        Expression? value = null;

        if (_tokens.Peek().Type == TokenType.Assign)
        {
            _tokens.Advance();
            value = ParseExpression();
        }

        return new VariableDeclaration(name, value);
    }

    /// <summary>
    /// Разбирает объявление константы.
    /// Правила: constant_declaration = "const", identifier, "=", expression ;
    /// </summary>
    private AstNode ParseConstantDeclaration()
    {
        Match(TokenType.Const);
        string name = ParseIdentifier();
        Match(TokenType.Assign);
        Expression value = ParseExpression();

        return new ConstantDeclaration(name, value);
    }

    /// <summary>
    /// Разбирает инструкцию ввода.
    /// Правила: input_statement = "input", "(", identifier, ")" ;
    /// </summary>
    private AstNode ParseInputStatement()
    {
        Match(TokenType.Input);
        Match(TokenType.OpenParenthesis);
        string variableName = ParseIdentifier();
        Match(TokenType.CloseParenthesis);

        return new InputExpression(variableName);
    }

    /// <summary>
    /// Разбирает присваивание или вызов функции.
    /// </summary>
    private AstNode ParseAssignmentOrFunctionCall()
    {
        string name = ParseIdentifier();

        if (_tokens.Peek().Type == TokenType.Assign)
        {
            return ParseAssignment(name);
        }

        if (_tokens.Peek().Type == TokenType.OpenParenthesis)
        {
            _tokens.Advance();

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
            Match(TokenType.Semicolon);

            return new FunctionCall(name, arguments);
        }

        throw new UnexpectedLexemeException("assignment or function call", _tokens.Peek());
    }

    /// <summary>
    /// Разбирает инструкцию присваивания.
    /// Правила: assignment_statement = identifier, "=", expression ;
    /// </summary>
    private AstNode ParseAssignment(string variableName)
    {
        Match(TokenType.Assign);
        Expression value = ParseExpression();
        Match(TokenType.Semicolon);

        return new AssignmentExpression(variableName, value);
    }

    /// <summary>
    /// Разбирает инструкцию вывода.
    /// Правила: print_statement = "print", "(", [ argument_list ], ")" ;
    /// </summary>
    private AstNode ParsePrintStatement()
    {
        Match(TokenType.Print);
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

        return new PrintExpression(arguments);
    }

    /// <summary>
    /// Разбирает инструкцию if.
    /// Использует подход matched/unmatched для решения проблемы висячего else.
    /// </summary>
    private AstNode ParseIfStatement()
    {
        Match(TokenType.If);
        Match(TokenType.OpenParenthesis);
        Expression condition = ParseExpression();
        Match(TokenType.CloseParenthesis);

        // Парсим блок напрямую, используя универсальный ParseStatement
        Match(TokenType.OpenBrace);
        List<AstNode> thenBranch = [];
        while (_tokens.Peek().Type != TokenType.CloseBrace && _tokens.Peek().Type != TokenType.EndOfFile)
        {
            AstNode node = ParseStatement();
            thenBranch.Add(node);
        }
        Match(TokenType.CloseBrace);

        if (_tokens.Peek().Type == TokenType.Else)
        {
            Match(TokenType.Else);
            Match(TokenType.OpenBrace);
            List<AstNode> elseBranch = [];
            while (_tokens.Peek().Type != TokenType.CloseBrace && _tokens.Peek().Type != TokenType.EndOfFile)
            {
                AstNode node = ParseStatement();
                elseBranch.Add(node);
            }
            Match(TokenType.CloseBrace);
            return new IfElseExpression(condition, thenBranch, elseBranch);
        }

        return new IfExpression(condition, thenBranch);
    }

    /// <summary>
    /// Разбирает инструкцию while.
    /// Правила: while_statement = "while", "(", expression, ")", loop_block ;
    /// </summary>
    private AstNode ParseWhileStatement()
    {
        Match(TokenType.While);
        Match(TokenType.OpenParenthesis);
        Expression condition = ParseExpression();
        Match(TokenType.CloseParenthesis);

        // Парсим блок напрямую, используя универсальный ParseStatement
        Match(TokenType.OpenBrace);
        List<AstNode> body = [];
        while (_tokens.Peek().Type != TokenType.CloseBrace && _tokens.Peek().Type != TokenType.EndOfFile)
        {
            AstNode node = ParseStatement();
            body.Add(node);
        }
        Match(TokenType.CloseBrace);

        return new WhileExpression(condition, body);
    }

    /// <summary>
    /// Разбирает инструкцию for.
    /// Правила: for_statement = "for", "(", assignment_statement, ( "to" | "downto" ), expression, ")", loop_block ;
    /// </summary>
    private AstNode ParseForStatement()
    {
        Match(TokenType.For);
        Match(TokenType.OpenParenthesis);

        string iteratorName = ParseIdentifier();
        Match(TokenType.Assign);
        Expression startValue = ParseExpression();

        bool isDownto = false;
        if (_tokens.Peek().Type == TokenType.To)
        {
            _tokens.Advance();
        }
        else if (_tokens.Peek().Type == TokenType.Downto)
        {
            _tokens.Advance();
            isDownto = true;
        }
        else
        {
            throw new UnexpectedLexemeException("to or downto", _tokens.Peek());
        }

        Expression endValue = ParseExpression();
        Match(TokenType.CloseParenthesis);

        // Парсим блок напрямую, используя универсальный ParseStatement
        Match(TokenType.OpenBrace);
        List<AstNode> body = [];
        while (_tokens.Peek().Type != TokenType.CloseBrace && _tokens.Peek().Type != TokenType.EndOfFile)
        {
            AstNode node = ParseStatement();
            body.Add(node);
        }
        Match(TokenType.CloseBrace);

        Expression iteratorVar = new VariableExpression(iteratorName);
        BinaryOperation comparisonOp = isDownto ? BinaryOperation.GreaterThanOrEqual : BinaryOperation.LessThanOrEqual;
        Expression endCondition = new BinaryOperationExpression(iteratorVar, comparisonOp, endValue);

        Expression oneLiteral = new LiteralExpression(1.0);
        BinaryOperation stepOp = isDownto ? BinaryOperation.Minus : BinaryOperation.Plus;
        Expression stepExpression = new BinaryOperationExpression(iteratorVar, stepOp, oneLiteral);
        Expression stepValue = new AssignmentExpression(iteratorName, stepExpression);

        return new ForLoopExpression(iteratorName, startValue, endCondition, stepValue, body);
    }

    /// <summary>
    /// Разбирает блок инструкций.
    /// Правила: block = "{", { statement }, "}" ;
    /// Используется для парсинга блоков верхнего уровня (не внутри функций).
    /// </summary>
    private List<AstNode> ParseBlock()
    {
        Match(TokenType.OpenBrace);
        List<AstNode> nodes = [];

        while (_tokens.Peek().Type != TokenType.CloseBrace && _tokens.Peek().Type != TokenType.EndOfFile)
        {
            AstNode node = ParseStatement();
            nodes.Add(node);
        }

        Match(TokenType.CloseBrace);
        return nodes;
    }


    /// <summary>
    /// Разбирает инструкцию break.
    /// Правила: break_statement = "break" ;
    /// </summary>
    private AstNode ParseBreakStatement()
    {
        Match(TokenType.Break);
        return new BreakExpression();
    }

    /// <summary>
    /// Разбирает инструкцию continue.
    /// Правила: continue_statement = "continue" ;
    /// </summary>
    private AstNode ParseContinueStatement()
    {
        Match(TokenType.Continue);
        return new ContinueExpression();
    }

    /// <summary>
    /// Разбирает инструкцию return.
    /// Правила: return_statement = "return", [ expression ], ";" ;
    /// </summary>
    private AstNode ParseReturnStatement()
    {
        Match(TokenType.Return);
        
        Expression? value = null;
        if (_tokens.Peek().Type != TokenType.Semicolon)
        {
            value = ParseExpression();
        }

        Match(TokenType.Semicolon);

        return new ReturnExpression(value);
    }

    /// <summary>
    /// Разбирает выражение.
    /// Правила: expression = logical_or ;
    /// </summary>
    private Expression ParseExpression()
    {
        return ParseLogicalOr();
    }

    /// <summary>
    /// Разбирает логическое ИЛИ выражение.
    /// Правила: logical_or = logical_and, { "||", logical_and } ;
    /// </summary>
    private Expression ParseLogicalOr()
    {
        Expression left = ParseLogicalAnd();

        while (_tokens.Peek().Type == TokenType.Or)
        {
            _tokens.Advance();
            Expression right = ParseLogicalAnd();
            left = new BinaryOperationExpression(left, BinaryOperation.LogicalOr, right);
        }

        return left;
    }

    /// <summary>
    /// Разбирает логическое И выражение.
    /// Правила: logical_and = equality, { "&&", equality } ;
    /// </summary>
    private Expression ParseLogicalAnd()
    {
        Expression left = ParseEquality();

        while (_tokens.Peek().Type == TokenType.And)
        {
            _tokens.Advance();
            Expression right = ParseEquality();
            left = new BinaryOperationExpression(left, BinaryOperation.LogicalAnd, right);
        }

        return left;
    }

    /// <summary>
    /// Разбирает выражение равенства/неравенства.
    /// Правила: equality = comparison, { ( "==" | "!=" ), comparison } ;
    /// </summary>
    private Expression ParseEquality()
    {
        Expression left = ParseComparison();

        while (_tokens.Peek().Type == TokenType.Equal || _tokens.Peek().Type == TokenType.NotEqual)
        {
            Token operatorToken = _tokens.Peek();
            _tokens.Advance();

            Expression right = ParseComparison();
            BinaryOperation operation = operatorToken.Type == TokenType.Equal
                ? BinaryOperation.Equal
                : BinaryOperation.NotEqual;
            left = new BinaryOperationExpression(left, operation, right);
        }

        return left;
    }

    /// <summary>
    /// Разбирает выражение сравнения.
    /// Правила: comparison = additive, { ( "<" | "<=" | ">" | ">=" ), additive } ;
    /// </summary>
    private Expression ParseComparison()
    {
        Expression left = ParseAdditive();

        while (IsComparisonOperator(_tokens.Peek().Type))
        {
            Token operatorToken = _tokens.Peek();
            _tokens.Advance();

            Expression right = ParseAdditive();
            BinaryOperation operation = operatorToken.Type switch
            {
                TokenType.Less => BinaryOperation.LessThan,
                TokenType.Greater => BinaryOperation.GreaterThan,
                TokenType.LessOrEqual => BinaryOperation.LessThanOrEqual,
                TokenType.GreaterOrEqual => BinaryOperation.GreaterThanOrEqual,
                _ => throw new Exception($"Unsupported comparison operator: {operatorToken.Type}"),
            };
            left = new BinaryOperationExpression(left, operation, right);
        }

        return left;
    }

    /// <summary>
    /// Разбирает аддитивное выражение.
    /// Правила: additive = multiplicative, { ( "+" | "-" ), multiplicative } ;
    /// </summary>
    private Expression ParseAdditive()
    {
        Expression left = ParseMultiplicative();

        while (_tokens.Peek().Type == TokenType.Plus || _tokens.Peek().Type == TokenType.Minus)
        {
            Token operatorToken = _tokens.Peek();
            _tokens.Advance();

            Expression right = ParseMultiplicative();
            BinaryOperation operation = operatorToken.Type == TokenType.Plus
                ? BinaryOperation.Plus
                : BinaryOperation.Minus;
            left = new BinaryOperationExpression(left, operation, right);
        }

        return left;
    }

    /// <summary>
    /// Разбирает мультипликативное выражение.
    /// Правила: multiplicative = power, { ( "*" | "/" | "//" | "%" ), power } ;
    /// </summary>
    private Expression ParseMultiplicative()
    {
        Expression left = ParsePower();

        while (IsMultiplicativeOperator(_tokens.Peek().Type))
        {
            Token operatorToken = _tokens.Peek();
            _tokens.Advance();

            Expression right = ParsePower();
            BinaryOperation operation = operatorToken.Type switch
            {
                TokenType.Multiply => BinaryOperation.Multiply,
                TokenType.Divide => BinaryOperation.Divide,
                TokenType.IntegerDivide => BinaryOperation.IntegerDivide,
                TokenType.Modulo => BinaryOperation.Modulo,
                _ => throw new Exception($"Unsupported multiplicative operator: {operatorToken.Type}"),
            };
            left = new BinaryOperationExpression(left, operation, right);
        }

        return left;
    }

    /// <summary>
    /// Разбирает выражение возведения в степень.
    /// Правила: power = unary, [ "^", power ] ;
    /// Оператор ^ правоассоциативный
    /// </summary>
    private Expression ParsePower()
    {
        Expression left = ParseUnary();

        if (_tokens.Peek().Type == TokenType.Power)
        {
            _tokens.Advance();
            Expression right = ParsePower(); // Рекурсия для правой ассоциативности
            left = new BinaryOperationExpression(left, BinaryOperation.Power, right);
        }

        return left;
    }

    /// <summary>
    /// Разбирает унарное выражение.
    /// Правила: unary = "+" , unary | "-" , unary | "!" , unary | primary ;
    /// </summary>
    private Expression ParseUnary()
    {
        if (_tokens.Peek().Type == TokenType.Plus)
        {
            _tokens.Advance();
            Expression operand = ParseUnary();
            return new UnaryOperationExpression(UnaryOperation.Plus, operand);
        }

        if (_tokens.Peek().Type == TokenType.Minus)
        {
            _tokens.Advance();
            Expression operand = ParseUnary();
            return new UnaryOperationExpression(UnaryOperation.Minus, operand);
        }

        if (_tokens.Peek().Type == TokenType.Not)
        {
            _tokens.Advance();
            Expression operand = ParseUnary();
            return new UnaryOperationExpression(UnaryOperation.LogicalNot, operand);
        }

        return ParsePrimary();
    }

    /// <summary>
    /// Разбирает первичное выражение.
    /// Правила: primary = number | identifier | function_call_expression | "(", expression, ")" ;
    /// </summary>
    private Expression ParsePrimary()
    {
        Token token = _tokens.Peek();

        switch (token.Type)
        {
            case TokenType.IntegerLiteral:
            case TokenType.FloatLiteral:
                _tokens.Advance();
                double value = Convert.ToDouble(token.Value!.ToDecimal());
                return new LiteralExpression(value);

            case TokenType.Identifier:
                return ParseFunctionCallOrIdentifier();

            case TokenType.OpenParenthesis:
                _tokens.Advance();
                Expression result = ParseExpression();
                Match(TokenType.CloseParenthesis);
                return result;

            default:
                throw new UnexpectedLexemeException("primary expression", token);
        }
    }

    /// <summary>
    /// Разбирает вызов функции или идентификатор.
    /// Правила: function_call_expression = (built_in_function | identifier), "(", [ argument_list ], ")" ;
    /// </summary>
    private Expression ParseFunctionCallOrIdentifier()
    {
        string name = ParseIdentifier();

        if (_tokens.Peek().Type == TokenType.OpenParenthesis)
        {
            _tokens.Advance();

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
            return new FunctionCall(name, arguments);
        }

        return new VariableExpression(name);
    }


    private string ParseIdentifier()
    {
        Token token = _tokens.Peek();
        if (token.Type == TokenType.Identifier)
        {
            _tokens.Advance();
            return token.Value?.ToString() ?? throw new InvalidOperationException("Identifier value cannot be null");
        }

        throw new UnexpectedLexemeException("identifier", token);
    }

    private bool IsComparisonOperator(TokenType type)
    {
        return type == TokenType.Less || type == TokenType.Greater ||
               type == TokenType.LessOrEqual || type == TokenType.GreaterOrEqual;
    }

    private bool IsMultiplicativeOperator(TokenType type)
    {
        return type == TokenType.Multiply || type == TokenType.Divide ||
               type == TokenType.IntegerDivide || type == TokenType.Modulo;
    }

    private void Match(TokenType expected)
    {
        Token t = _tokens.Peek();
        if (t.Type != expected)
        {
            throw new UnexpectedLexemeException(expected, t);
        }
        _tokens.Advance();
    }
}
