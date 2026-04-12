using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Semantics.Exceptions;

namespace Semantics.Passes;

/// <summary>
/// Проход проверки типов: использует уже вычисленные ResultType
/// и проверяет совместимость.
/// Аналогично pstiger CheckTypesPass: base.Visit сначала, потом проверка.
/// </summary>
public sealed class CheckTypesPass : AbstractPass
{
    public override void Visit(VariableDeclarationExpression e)
    {
        base.Visit(e);

        if (e.Initializer is not null)
        {
            EnsureAssignable(e.Type, e.Initializer.ResultType, e.Name);
        }
    }

    public override void Visit(ConstantDeclarationExpression e)
    {
        base.Visit(e);
        EnsureAssignable(e.Type, e.Initializer.ResultType, e.Name);
    }

    public override void Visit(AssignmentExpression e)
    {
        base.Visit(e);
        EnsureAssignable(e.Symbol.Type, e.Value.ResultType, e.Name);
    }

    public override void Visit(CallExpression e)
    {
        base.Visit(e);
        CheckBuiltinArgumentTypes(e);
    }

    public override void Visit(ReturnExpression e)
    {
        base.Visit(e);

        if (e.Value.ResultType != DataType.Int)
        {
            throw new TypeErrorException("Main function must return a value of type int.");
        }
    }

    /// <summary>
    /// Проверка количества и типов аргументов встроенных функций.
    /// Аналогично pstiger CheckFunctionArgumentTypes.
    /// </summary>
    private static void CheckBuiltinArgumentTypes(CallExpression call)
    {
        BuiltinInfo builtin = call.Builtin;

        if (builtin.FixedArgCount is int expectedCount)
        {
            if (call.Arguments.Count != expectedCount)
            {
                throw new TypeErrorException(
                    $"'{builtin.Name}' expects {expectedCount} argument(s), got {call.Arguments.Count}.");
            }
        }
        else
        {
            // Variadic функции: min, max
            if (call.Arguments.Count < 2)
            {
                throw new TypeErrorException(
                    $"'{builtin.Name}' expects at least 2 arguments, got {call.Arguments.Count}.");
            }
        }

        switch (builtin.Name)
        {
            case "len":
                CheckAreSameTypes("'len'", call.Arguments[0].ResultType, DataType.String);
                break;

            case "substr":
                CheckAreSameTypes("'substr' arg 1", call.Arguments[0].ResultType, DataType.String);
                CheckAreSameTypes("'substr' arg 2", call.Arguments[1].ResultType, DataType.Int);
                CheckAreSameTypes("'substr' arg 3", call.Arguments[2].ResultType, DataType.Int);
                break;

            case "abs":
                CheckIsNumeric("'abs'", call.Arguments[0].ResultType);
                break;

            case "min":
            case "max":
                CheckAllNumeric(builtin.Name, call.Arguments);
                CheckAllSameTypes(builtin.Name, call.Arguments);
                break;
        }
    }

    private static void EnsureAssignable(DataType expected, DataType actual, string context)
    {
        if (expected != actual)
        {
            throw new TypeErrorException(
                $"Cannot assign value of type {actual} to '{context}' of type {expected}.");
        }
    }

    private static void CheckAreSameTypes(string context, DataType actual, DataType expected)
    {
        if (actual != expected)
        {
            throw new TypeErrorException($"'{context}' expects {expected}, got {actual}.");
        }
    }

    private static void CheckAllSameTypes(string functionName, List<Expression> arguments)
    {
        DataType firstType = arguments[0].ResultType;

        for (int i = 1; i < arguments.Count; i++)
        {
            if (arguments[i].ResultType != firstType)
            {
                throw new TypeErrorException(
                    $"'{functionName}' requires all arguments to be of the same type, " +
                    $"expected {firstType}, got {arguments[i].ResultType} at argument {i + 1}.");
            }
        }
    }

    private static void CheckIsNumeric(string functionName, DataType type)
    {
        if (type is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException($"'{functionName}' expects int/num, got {type}.");
        }
    }

    private static void CheckAllNumeric(string functionName, List<Expression> arguments)
    {
        for (int i = 0; i < arguments.Count; i++)
        {
            if (arguments[i].ResultType is not (DataType.Int or DataType.Num))
            {
                throw new TypeErrorException(
                    $"'{functionName}' expects int/num arguments, got {arguments[i].ResultType}.");
            }
        }
    }
}