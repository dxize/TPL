namespace Ast.Expressions;

public sealed class IfElseExpression : Expression
{
    public IfElseExpression(Expression condition, List<AstNode> thenBranch, List<AstNode> elseBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }

    public Expression Condition { get; }

    public List<AstNode> ThenBranch { get; }

    public List<AstNode> ElseBranch { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}

