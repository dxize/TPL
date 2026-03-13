using Ast.Expressions;

namespace Semantics.Passes;

/// <summary>
/// Во 2-й итерации типы литералов уже заданы прямо в AST,
/// поэтому этот проход пока оставлен как no-op для соответствия шаблону pstiger.
/// </summary>
public sealed class ResolveTypesPass : AbstractPass
{
    public override void Visit(LiteralExpression e)
    {
        base.Visit(e);
    }
}