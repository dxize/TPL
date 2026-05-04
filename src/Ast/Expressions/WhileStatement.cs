namespace Ast.Expressions;

public sealed class WhileStatement : AstNode
{
    public WhileStatement(Expression condition, List<AstNode> body)
    {
        Condition = condition;
        Body = body;
    }

    public Expression Condition { get; }

    public List<AstNode> Body { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}