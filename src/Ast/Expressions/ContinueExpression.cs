namespace Ast.Expressions;

public sealed class ContinueExpression : Expression
{
    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}

