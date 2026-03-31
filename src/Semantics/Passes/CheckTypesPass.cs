using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Semantics.Exceptions;

namespace Semantics.Passes;

/// <summary>
/// Проверка типов для 3-й итерации с поддержкой глобальных переменных.
/// </summary>
public sealed class CheckTypesPass : AbstractPass
{
    /// <summary>
    /// Глобальные типы: имя -> тип
    /// </summary>
    private readonly Dictionary<string, DataType> _globalTypes = new(StringComparer.Ordinal);

    /// <summary>
    /// Глобальные константы: имя -> isConst
    /// </summary>
    private readonly Dictionary<string, bool> _globalConstness = new(StringComparer.Ordinal);

    /// <summary>
    /// Локальные типы (для функции)
    /// </summary>
    private Dictionary<string, DataType>? _localTypes;

    /// <summary>
    /// Локальные константы
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
            throw new TypeErrorException($"Нельзя присваивать значение константе '{e.Name}'.");
        }

        DataType valueType = ResolveExpressionType(e.Value);
        EnsureAssignable(expectedType, valueType, e.Name);
    }

    public override void Visit(InputExpression e)
    {
        if (!HasTypeForName(e.VariableName))
        {
            throw new TypeErrorException($"Идентификатор '{e.VariableName}' не объявлен.");
        }
    }

    public override void Visit(CallExpression e)
    {
        ResolveCallType(e);
        base.Visit(e);
    }

    public override void Visit(ReturnExpression e)
    {
        DataType type = ResolveExpressionType(e.Value);
        if (type != DataType.Int)
        {
            throw new TypeErrorException("main должна возвращать значение типа int.");
        }
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

        throw new TypeErrorException($"Идентификатор '{name}' не объявлен.");
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

    private DataType ResolveExpressionType(Expression e)
    {
        return e switch
        {
            LiteralExpression literal => literal.Type,
            IdentifierExpression identifier => ResolveIdentifierType(identifier),
            UnaryExpression unary => ResolveUnaryType(unary),
            BinaryExpression binary => ResolveBinaryType(binary),
            CallExpression call => ResolveCallType(call),
            _ => throw new TypeErrorException($"Неподдерживаемое выражение: {e.GetType().Name}."),
        };
    }

    private DataType ResolveIdentifierType(IdentifierExpression e)
    {
        return GetTypeForName(e.Name);
    }

    private DataType ResolveUnaryType(UnaryExpression e)
    {
        DataType operandType = ResolveExpressionType(e.Operand);
        if (operandType is DataType.Int or DataType.Num)
        {
            return operandType;
        }

        throw new TypeErrorException("Унарные + и - поддерживаются только для int/num.");
    }

    private DataType ResolveBinaryType(BinaryExpression e)
    {
        DataType left = ResolveExpressionType(e.Left);
        DataType right = ResolveExpressionType(e.Right);

        // Операторы сравнения возвращают int (1 = true, 0 = false)
        if (e.OperatorKind is OperatorKind.Less or OperatorKind.LessOrEqual or 
            OperatorKind.Greater or OperatorKind.GreaterOrEqual)
        {
            if (left is DataType.Int or DataType.Num && right is DataType.Int or DataType.Num)
            {
                return DataType.Int;
            }
            throw new TypeErrorException($"Оператор '{e.OperatorKind}' поддерживает только int/num.");
        }

        if (e.OperatorKind is OperatorKind.Equal or OperatorKind.NotEqual)
        {
            // == и != поддерживают int, num, string
            if (left == right)
            {
                return DataType.Int;
            }
            // int и num можно сравнивать
            if ((left == DataType.Int && right == DataType.Num) || 
                (left == DataType.Num && right == DataType.Int))
            {
                return DataType.Int;
            }
            throw new TypeErrorException($"Оператор '{e.OperatorKind}' не поддерживает типы {left} и {right}.");
        }

        if (e.OperatorKind == OperatorKind.Plus && left == DataType.String && right == DataType.String)
        {
            return DataType.String;
        }

        if (left is DataType.Int or DataType.Num && right is DataType.Int or DataType.Num)
        {
            if (e.OperatorKind is OperatorKind.IntegerDivide or OperatorKind.Modulo)
            {
                if (left != DataType.Int || right != DataType.Int)
                {
                    throw new TypeErrorException("Операции // и % поддерживаются только для int.");
                }

                return DataType.Int;
            }

            if (left == DataType.Int && right == DataType.Int && e.OperatorKind is not OperatorKind.Divide)
            {
                return DataType.Int;
            }

            return DataType.Num;
        }

        throw new TypeErrorException($"Операция {e.OperatorKind} не поддерживается для типов {left} и {right}.");
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
            _ => throw new TypeErrorException($"Неизвестная функция '{call.Name}'."),
        };
    }

    private DataType ResolveLenType(CallExpression call)
    {
        if (call.Arguments.Count != 1)
        {
            throw new InvalidBuiltinCallException("len ожидает 1 аргумент.");
        }

        if (ResolveExpressionType(call.Arguments[0]) != DataType.String)
        {
            throw new TypeErrorException("len поддерживает только string.");
        }

        return DataType.Int;
    }

    private DataType ResolveSubstrType(CallExpression call)
    {
        if (call.Arguments.Count != 3)
        {
            throw new InvalidBuiltinCallException("substr ожидает 3 аргумента.");
        }

        if (ResolveExpressionType(call.Arguments[0]) != DataType.String ||
            ResolveExpressionType(call.Arguments[1]) != DataType.Int ||
            ResolveExpressionType(call.Arguments[2]) != DataType.Int)
        {
            throw new TypeErrorException("substr ожидает (string, int, int).");
        }

        return DataType.String;
    }

    private DataType ResolveAbsType(CallExpression call)
    {
        if (call.Arguments.Count != 1)
        {
            throw new InvalidBuiltinCallException("abs ожидает 1 аргумент.");
        }

        DataType type = ResolveExpressionType(call.Arguments[0]);
        if (type is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException("abs поддерживает только int/num.");
        }

        return type;
    }

    private DataType ResolveMinMaxType(CallExpression call, string functionName)
    {
        if (call.Arguments.Count < 2)
        {
            throw new InvalidBuiltinCallException($"{functionName} ожидает минимум 2 аргумента.");
        }

        DataType first = ResolveExpressionType(call.Arguments[0]);
        if (first is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException($"{functionName} поддерживает только int/num.");
        }

        for (int i = 1; i < call.Arguments.Count; i++)
        {
            DataType type = ResolveExpressionType(call.Arguments[i]);
            if (type != first)
            {
                throw new TypeErrorException($"{functionName} требует аргументы одного типа.");
            }
        }

        return first;
    }

    private static void EnsureAssignable(DataType expectedType, DataType actualType, string variableName)
    {
        if (expectedType != actualType)
        {
            throw new TypeErrorException($"Тип значения несовместим с '{variableName}'.");
        }
    }
}