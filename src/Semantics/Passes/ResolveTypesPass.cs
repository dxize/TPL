using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Semantics.Exceptions;

namespace Semantics.Passes;

public sealed class ResolveTypesPass : AbstractPass
{
    public override void Visit(LiteralExpression e)
    {
        e.ResultType = e.Type;
    }

    public override void Visit(IdentifierExpression e)
    {
        e.ResultType = e.Symbol.Type;
    }

    public override void Visit(UnaryExpression e)
    {
        base.Visit(e);
        DataType operandType = e.Operand.ResultType;

        if (e.OperatorKind == OperatorKind.Not)
        {
            EnsureBoolConvertible(operandType, "logical negation");
            e.ResultType = DataType.Bool;
            return;
        }

        if (operandType is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException($"Unary '{e.OperatorKind}' is only supported for int/num, got {operandType}.");
        }

        e.ResultType = operandType;
    }

    public override void Visit(BinaryExpression e)
    {
        base.Visit(e);
        e.ResultType = ComputeBinaryResultType(e.OperatorKind, e.Left.ResultType, e.Right.ResultType);
    }

    public override void Visit(CallExpression e)
    {
        base.Visit(e);
        e.ResultType = e.IsBuiltin
            ? ComputeBuiltinReturnType(e)
            : e.Function!.ReturnType;
    }

    public override void Visit(ProcedureCallStatement s)
    {
        base.Visit(s);
    }

    public override void Visit(InputExpression e)
    {
        e.ResultType = e.Symbol.Type;
    }

    public override void Visit(PrintExpression e)
    {
        base.Visit(e);
        e.ResultType = DataType.Void;
    }

    public override void Visit(AssignmentExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Symbol.Type;
    }

    public override void Visit(VariableDeclarationExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Type;
    }

    public override void Visit(ConstantDeclarationExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Type;
    }

    public override void Visit(ReturnExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Value?.ResultType ?? DataType.Void;
    }

    public override void Visit(IfStatement s)
    {
        base.Visit(s);
    }

    private static DataType ComputeBuiltinReturnType(CallExpression call)
    {
        return call.Name switch
        {
            "len" => DataType.Int,
            "substr" => DataType.String,
            "abs" => call.Arguments.Count > 0 ? call.Arguments[0].ResultType : DataType.Int,
            "min" => call.Arguments.Count > 0 ? call.Arguments[0].ResultType : DataType.Int,
            "max" => call.Arguments.Count > 0 ? call.Arguments[0].ResultType : DataType.Int,
            _ => throw new TypeErrorException($"Unknown function '{call.Name}'."),
        };
    }

    private static DataType ComputeBinaryResultType(OperatorKind op, DataType left, DataType right)
    {
        if (op is OperatorKind.And or OperatorKind.Or)
        {
            EnsureBoolConvertible(left, op.ToString());
            EnsureBoolConvertible(right, op.ToString());
            return DataType.Bool;
        }

        if (op is OperatorKind.Equal or OperatorKind.NotEqual)
        {
            if (AreComparableForEquality(left, right))
            {
                return DataType.Bool;
            }

            throw new TypeErrorException($"Operator '{op}' is not supported for types {left} and {right}.");
        }

        if (op is OperatorKind.Less or OperatorKind.LessOrEqual or OperatorKind.Greater or OperatorKind.GreaterOrEqual)
        {
            if (IsNumeric(left) && IsNumeric(right))
            {
                return DataType.Bool;
            }

            throw new TypeErrorException($"Operator '{op}' is only supported for int/num.");
        }

        if (op == OperatorKind.Plus && left == DataType.String && right == DataType.String)
        {
            return DataType.String;
        }

        if (!IsNumeric(left) || !IsNumeric(right))
        {
            throw new TypeErrorException($"Operator '{op}' is not supported for types {left} and {right}.");
        }

        if (op is OperatorKind.IntegerDivide or OperatorKind.Modulo)
        {
            if (left != DataType.Int || right != DataType.Int)
            {
                throw new TypeErrorException($"Operator '{op}' is only supported for int.");
            }

            return DataType.Int;
        }

        if (op == OperatorKind.Divide)
        {
            return DataType.Num;
        }

        if (left == DataType.Int && right == DataType.Int)
        {
            return DataType.Int;
        }

        return DataType.Num;
    }

    private static bool IsNumeric(DataType type) => type is DataType.Int or DataType.Num;

    private static bool AreComparableForEquality(DataType left, DataType right)
    {
        return (IsNumeric(left) && IsNumeric(right)) || left == right;
    }

    private static void EnsureBoolConvertible(DataType type, string context)
    {
        if (type is not (DataType.Bool or DataType.Int or DataType.Num or DataType.String))
        {
            throw new TypeErrorException($"{context} expects bool-convertible value, got {type}.");
        }
    }
}