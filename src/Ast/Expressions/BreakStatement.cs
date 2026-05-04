namespace Ast.Expressions;

public sealed class BreakStatement : AstNode
{
    public BreakStatement()
    {
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}