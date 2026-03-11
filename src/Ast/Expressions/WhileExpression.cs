namespace Ast.Expressions;

public sealed class WhileExpression : Expression
{
    public WhileExpression(Expression condition, List<AstNode> body)
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

