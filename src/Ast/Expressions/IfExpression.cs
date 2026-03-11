namespace Ast.Expressions;

public sealed class IfExpression : Expression
{
    public IfExpression(Expression condition, List<AstNode> thenBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
    }

    public Expression Condition { get; }

    public List<AstNode> ThenBranch { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}

