using Ast;
using Ast.Expressions;

using Semantics.Exceptions;

namespace Semantics.Passes;

/// <summary>
/// Проверяет корректность типов во 2-й итерации:
/// - допустимы только int / num / string литералы
/// - print принимает только такие литералы
/// - return должен возвращать int-литерал
/// </summary>
public sealed class CheckTypesPass : AbstractPass
{
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
                    $"Во 2-й итерации тип {e.Type} не поддерживается.");
        }
    }

    public override void Visit(PrintExpression e)
    {
        foreach (LiteralExpression argument in e.Arguments)
        {
            argument.Accept(this);

            if (argument.Type is not (DataType.Int or DataType.Num or DataType.String))
            {
                throw new TypeErrorException(
                    "print(...) во 2-й итерации принимает только литералы типов int, num и string.");
            }
        }
    }

    public override void Visit(ReturnExpression e)
    {
        e.Value.Accept(this);

        if (e.Value.Type != DataType.Int)
        {
            throw new TypeErrorException(
                "Во 2-й итерации return должен возвращать int-литерал.");
        }
    }
}