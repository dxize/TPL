using Ast;
using Ast.Expressions;

using Semantics.Exceptions;

namespace Semantics.Passes;

/// <summary>
/// Минимальные проверки типов для 3-й итерации (без bool):
/// - допустимы только int / num / string литералы
/// Остальные проверки выполняются на этапе интерпретации.
/// </summary>
public sealed class CheckTypesPass : AbstractPass
{
    private readonly Dictionary<string, DataType> _types = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _constness = new(StringComparer.Ordinal);

    public override void Visit(ProgramNode p)
    {
        _types.Clear();
        _constness.Clear();
        base.Visit(p);
    }

    public override void Visit(LiteralExpression e)
    {
        switch (e.Type)
        {
            case DataType.Int:
                if (e.Value is not int)
                {
                    throw new TypeErrorException(
                        "Литерал типа int должен содержать значение типа int.");
                }

                break;

            case DataType.Num:
                if (e.Value is not double)
                {
                    throw new TypeErrorException(
                        "Литерал типа num должен содержать значение типа double.");
                }

                break;

            case DataType.String:
                if (e.Value is not string)
                {
                    throw new TypeErrorException(
                        "Литерал типа string должен содержать значение типа string.");
                }

                break;

            default:
                throw new TypeErrorException(
                    $"Тип {e.Type} пока не поддерживается.");
        }
    }

    public override void Visit(VariableDeclarationExpression e)
    {
        if (e.Initializer is not null)
        {
            DataType initType = ResolveExpressionType(e.Initializer);
            EnsureAssignable(e.Type, initType, e.Name);
        }

        _types[e.Name] = e.Type;
        _constness[e.Name] = false;
    }

    public override void Visit(ConstantDeclarationExpression e)
    {
        DataType initType = ResolveExpressionType(e.Initializer);
        EnsureAssignable(e.Type, initType, e.Name);
        _types[e.Name] = e.Type;
        _constness[e.Name] = true;
    }

    public override void Visit(AssignmentExpression e)
    {
        if (!_types.TryGetValue(e.Name, out DataType expectedType))
        {
            throw new TypeErrorException($"Идентификатор '{e.Name}' не объявлен.");
        }

        if (_constness.TryGetValue(e.Name, out bool isConst) && isConst)
        {
            throw new TypeErrorException($"Нельзя присваивать значение константе '{e.Name}'.");
        }

        DataType valueType = ResolveExpressionType(e.Value);
        EnsureAssignable(expectedType, valueType, e.Name);
    }

    public override void Visit(InputExpression e)
    {
        if (!_types.ContainsKey(e.VariableName))
        {
            throw new TypeErrorException($"Идентификатор '{e.VariableName}' не объявлен.");
        }
    }

    public override void Visit(ReturnExpression e)
    {
        DataType type = ResolveExpressionType(e.Value);
        if (type != DataType.Int)
        {
            throw new TypeErrorException("main должна возвращать значение типа int.");
        }
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
        if (!_types.TryGetValue(e.Name, out DataType type))
        {
            throw new TypeErrorException($"Идентификатор '{e.Name}' не объявлен.");
        }

        return type;
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
            throw new TypeErrorException("len ожидает 1 аргумент.");
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
            throw new TypeErrorException("substr ожидает 3 аргумента.");
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
            throw new TypeErrorException("abs ожидает 1 аргумент.");
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
            throw new TypeErrorException($"{functionName} ожидает минимум 2 аргумента.");
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