using Ast.Declarations;
using Ast.Expressions;

namespace Semantics.Passes;

/// <summary>
/// Во 2-й итерации почти нечего резолвить по именам:
/// есть только main, литералы, print и return.
/// Проход оставлен для соответствия шаблону DEA.
/// </summary>
public sealed class ResolveNamesPass : AbstractPass
{
    public override void Visit(FunctionDeclaration d)
    {
        if (!string.Equals(d.Name, "main", StringComparison.Ordinal))
        {
            throw new Exceptions.InvalidExpressionException(
                "Во 2-й итерации поддерживается только точка входа func int main().");
        }

        base.Visit(d);
    }

    public override void Visit(PrintExpression e)
    {
        base.Visit(e);
    }

    public override void Visit(ReturnExpression e)
    {
        base.Visit(e);
    }
}