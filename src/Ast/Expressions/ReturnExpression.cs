namespace Ast.Expressions;

public sealed class ReturnExpression : Expression
{
    public ReturnExpression(LiteralExpression value)
    {
        Value = value;
    }

    public LiteralExpression Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}