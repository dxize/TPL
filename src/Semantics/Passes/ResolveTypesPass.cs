using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Semantics.Exceptions;

namespace Semantics.Passes;

/// <summary>
/// Проход вычисления типов: записывает ResultType на каждый узел.
/// </summary>
public sealed class ResolveTypesPass : AbstractPass
{
    public override void Visit(LiteralExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Type;
    }

    public override void Visit(IdentifierExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Symbol.Type;
    }

    public override void Visit(UnaryExpression e)
    {
        base.Visit(e);

        DataType operandType = e.Operand.ResultType;

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
        e.ResultType = ComputeCallReturnType(e);
    }

    public override void Visit(InputExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Symbol.Type;
    }

    public override void Visit(PrintExpression e)
    {
        base.Visit(e);
        e.ResultType = DataType.Int;
    }

    public override void Visit(AssignmentExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Symbol.Type;
    }

    public override void Visit(VariableDeclarationExpression e)
    {
        // base.Visit сначала — обходим инициализатор
        base.Visit(e);
        e.ResultType = e.Type;
    }

    public override void Visit(ConstantDeclarationExpression e)
    {
        // base.Visit сначала — обходим инициализатор
        base.Visit(e);
        e.ResultType = e.Type;
    }

    public override void Visit(ReturnExpression e)
    {
        base.Visit(e);
        e.ResultType = e.Value.ResultType;
    }

    private static DataType ComputeCallReturnType(CallExpression call)
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
        if (op == OperatorKind.Plus && left == DataType.String && right == DataType.String)
        {
            return DataType.String;
        }

        if (left is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException($"Operator '{op}' is not supported for type {left}.");
        }

        if (right is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException($"Operator '{op}' is not supported for type {right}.");
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
}