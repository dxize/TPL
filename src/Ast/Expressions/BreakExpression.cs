namespace Ast.Expressions;

public sealed class BreakExpression : Expression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}

