using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Semantics.Exceptions;

namespace Semantics.Passes;

/// <summary>
/// Проход по AST для проверки корректности программы с точки зрения совместимости типов данных.
/// </summary>
public sealed class CheckTypesPass : AbstractPass
{
    /// <summary>
    /// Глобальные типы: имя -> тип данных.
    /// </summary>
    private readonly Dictionary<string, DataType> _globalTypes = new(StringComparer.Ordinal);

    /// <summary>
    /// Глобальные константы: имя -> isConst.
    /// </summary>
    private readonly Dictionary<string, bool> _globalConstness = new(StringComparer.Ordinal);

    /// <summary>
    /// Текущая локальная область видимости: имя -> тип данных.
    /// </summary>
    private Dictionary<string, DataType>? _localTypes;

    /// <summary>
    /// Текущая локальная область видимости: имя -> isConst.
    /// </summary>
    private Dictionary<string, bool>? _localConstness;

    public override void Visit(ProgramNode p)
    {
        _globalTypes.Clear();
        _globalConstness.Clear();
        _localTypes = null;
        _localConstness = null;

        foreach (Declaration decl in p.GlobalDeclarations)
        {
            decl.Accept(this);
        }

        p.MainFunction.Accept(this);
    }

    public override void Visit(FunctionDeclaration d)
    {
        _localTypes = new Dictionary<string, DataType>(StringComparer.Ordinal);
        _localConstness = new Dictionary<string, bool>(StringComparer.Ordinal);

        try
        {
            base.Visit(d);
        }
        finally
        {
            _localTypes = null;
            _localConstness = null;
        }
    }

    public override void Visit(VariableDeclarationExpression e)
    {
        if (e.Initializer is not null)
        {
            DataType initType = ResolveExpressionType(e.Initializer);
            EnsureAssignable(e.Type, initType, e.Name);
        }

        if (_localTypes != null)
        {
            _localTypes[e.Name] = e.Type;
            _localConstness![e.Name] = false;
        }
        else
        {
            _globalTypes[e.Name] = e.Type;
            _globalConstness[e.Name] = false;
        }
    }

    public override void Visit(ConstantDeclarationExpression e)
    {
        DataType initType = ResolveExpressionType(e.Initializer);
        EnsureAssignable(e.Type, initType, e.Name);

        if (_localTypes != null)
        {
            _localTypes[e.Name] = e.Type;
            _localConstness![e.Name] = true;
        }
        else
        {
            _globalTypes[e.Name] = e.Type;
            _globalConstness[e.Name] = true;
        }
    }

    public override void Visit(AssignmentExpression e)
    {
        DataType expectedType = GetTypeForName(e.Name);
        bool isConst = GetConstnessForName(e.Name);

        if (isConst)
        {
            throw new InvalidAssignmentException($"Cannot assign to constant '{e.Name}'.");
        }

        DataType valueType = ResolveExpressionType(e.Value);
        EnsureAssignable(expectedType, valueType, e.Name);
    }

    public override void Visit(InputExpression e)
    {
        if (!HasTypeForName(e.VariableName))
        {
            throw new TypeErrorException($"Identifier '{e.VariableName}' is not declared.");
        }
    }

    public override void Visit(CallExpression e)
    {
        ResolveCallType(e);
        base.Visit(e);
    }

    public override void Visit(PrintExpression e)
    {
        foreach (Expression argument in e.Arguments)
        {
            // print принимает любые типы — проверка не требуется
            ResolveExpressionType(argument);
        }
    }

    public override void Visit(ReturnExpression e)
    {
        DataType type = ResolveExpressionType(e.Value);
        if (type != DataType.Int)
        {
            throw new TypeErrorException("Main function must return a value of type int.");
        }
    }

    public override void Visit(UnaryExpression e)
    {
        DataType operandType = ResolveExpressionType(e.Operand);
        if (operandType is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException(
                $"Unary '{e.OperatorKind}' is only supported for int/num, got {operandType}.");
        }
    }

    public override void Visit(BinaryExpression e)
    {
        DataType leftType = ResolveExpressionType(e.Left);
        DataType rightType = ResolveExpressionType(e.Right);
        ResolveBinaryType(e.OperatorKind, leftType, rightType);
    }

    public override void Visit(LiteralExpression e)
    {
        // Литералы всегда имеют корректный тип
    }

    public override void Visit(IdentifierExpression e)
    {
        // Тип идентификатора уже проверен в ResolveNamesPass
    }

    private DataType ResolveExpressionType(Expression e)
    {
        return e switch
        {
            LiteralExpression literal => literal.Type,
            IdentifierExpression identifier => GetTypeForName(identifier.Name),
            UnaryExpression unary => ResolveUnaryType(unary),
            BinaryExpression binary => ResolveBinaryResultType(binary),
            CallExpression call => ResolveCallType(call),
            InputExpression => DataType.Int, // input всегда читает в объявленную переменную, тип проверен отдельно
            _ => throw new TypeErrorException($"Unsupported expression: {e.GetType().Name}."),
        };
    }

    private DataType ResolveUnaryType(UnaryExpression e)
    {
        DataType operandType = ResolveExpressionType(e.Operand);
        if (operandType is DataType.Int or DataType.Num)
        {
            return operandType;
        }

        throw new TypeErrorException($"Unary '{e.OperatorKind}' is only supported for int/num, got {operandType}.");
    }

    private DataType ResolveBinaryResultType(BinaryExpression e)
    {
        return ResolveBinaryType(e.OperatorKind, ResolveExpressionType(e.Left), ResolveExpressionType(e.Right));
    }

    private static DataType ResolveBinaryType(OperatorKind op, DataType left, DataType right)
    {
        // Строковая конкатенация
        if (op == OperatorKind.Plus && left == DataType.String && right == DataType.String)
        {
            return DataType.String;
        }

        // Числовые операции
        if (left is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException($"Operator '{op}' is not supported for type {left}.");
        }

        if (right is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException($"Operator '{op}' is not supported for type {right}.");
        }

        // Целочисленные операторы: //, %
        if (op is OperatorKind.IntegerDivide or OperatorKind.Modulo)
        {
            if (left != DataType.Int || right != DataType.Int)
            {
                throw new TypeErrorException($"Operator '{op}' is only supported for int.");
            }

            return DataType.Int;
        }

        // Деление: int/int -> num, num/num -> num, mixed -> num
        if (op == OperatorKind.Divide)
        {
            return DataType.Num;
        }

        // Остальные арифметические: если оба int -> int, иначе -> num
        if (left == DataType.Int && right == DataType.Int)
        {
            return DataType.Int;
        }

        return DataType.Num;
    }

    private DataType ResolveCallType(CallExpression call)
    {
        return call.Name switch
        {
            "len" => ResolveLenType(call),
            "substr" => ResolveSubstrType(call),
            "abs" => ResolveAbsType(call),
            "min" => ResolveMinMaxType(call, "min"),
            "max" => ResolveMinMaxType(call, "max"),
            _ => throw new TypeErrorException($"Unknown function '{call.Name}'."),
        };
    }

    private DataType ResolveLenType(CallExpression call)
    {
        if (call.Arguments.Count != 1)
        {
            throw new InvalidBuiltinCallException("'len' expects 1 argument.");
        }

        DataType argType = ResolveExpressionType(call.Arguments[0]);
        if (argType != DataType.String)
        {
            throw new TypeErrorException($"'len' expects string, got {argType}.");
        }

        return DataType.Int;
    }

    private DataType ResolveSubstrType(CallExpression call)
    {
        if (call.Arguments.Count != 3)
        {
            throw new InvalidBuiltinCallException("'substr' expects 3 arguments.");
        }

        DataType sourceType = ResolveExpressionType(call.Arguments[0]);
        DataType startType = ResolveExpressionType(call.Arguments[1]);
        DataType lengthType = ResolveExpressionType(call.Arguments[2]);

        if (sourceType != DataType.String)
        {
            throw new TypeErrorException($"'substr' first argument expects string, got {sourceType}.");
        }

        if (startType != DataType.Int)
        {
            throw new TypeErrorException($"'substr' second argument expects int, got {startType}.");
        }

        if (lengthType != DataType.Int)
        {
            throw new TypeErrorException($"'substr' third argument expects int, got {lengthType}.");
        }

        return DataType.String;
    }

    private DataType ResolveAbsType(CallExpression call)
    {
        if (call.Arguments.Count != 1)
        {
            throw new InvalidBuiltinCallException("'abs' expects 1 argument.");
        }

        DataType argType = ResolveExpressionType(call.Arguments[0]);
        if (argType is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException($"'abs' expects int/num, got {argType}.");
        }

        return argType;
    }

    private DataType ResolveMinMaxType(CallExpression call, string functionName)
    {
        if (call.Arguments.Count < 2)
        {
            throw new InvalidBuiltinCallException($"'{functionName}' expects at least 2 arguments.");
        }

        DataType firstType = ResolveExpressionType(call.Arguments[0]);
        if (firstType is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException($"'{functionName}' expects int/num arguments, got {firstType}.");
        }

        for (int i = 1; i < call.Arguments.Count; i++)
        {
            DataType argType = ResolveExpressionType(call.Arguments[i]);
            if (argType != firstType)
            {
                throw new TypeErrorException(
                    $"'{functionName}' requires all arguments to be of the same type, " +
                    $"expected {firstType}, got {argType} at argument {i + 1}.");
            }
        }

        return firstType;
    }

    private DataType GetTypeForName(string name)
    {
        if (_localTypes != null && _localTypes.TryGetValue(name, out DataType localType))
        {
            return localType;
        }

        if (_globalTypes.TryGetValue(name, out DataType globalType))
        {
            return globalType;
        }

        throw new TypeErrorException($"Identifier '{name}' is not declared.");
    }

    private bool GetConstnessForName(string name)
    {
        if (_localConstness != null && _localConstness.TryGetValue(name, out bool localIsConst))
        {
            return localIsConst;
        }

        if (_globalConstness.TryGetValue(name, out bool globalIsConst))
        {
            return globalIsConst;
        }

        return false;
    }

    private bool HasTypeForName(string name)
    {
        if (_localTypes != null && _localTypes.ContainsKey(name))
        {
            return true;
        }

        return _globalTypes.ContainsKey(name);
    }

    private static void EnsureAssignable(DataType expectedType, DataType actualType, string variableName)
    {
        if (expectedType != actualType)
        {
            throw new TypeErrorException(
                $"Cannot assign value of type {actualType} to variable '{variableName}' of type {expectedType}.");
        }
    }
}