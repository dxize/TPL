namespace Ast.Expressions;

public sealed class ContinueStatement : AstNode
{
    public ContinueStatement()
    {
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}