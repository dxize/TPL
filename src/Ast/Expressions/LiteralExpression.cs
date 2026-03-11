namespace Ast.Expressions;

public sealed class LiteralExpression : Expression
{
    public LiteralExpression(double value)
    {
        Value = value;
    }

    public double Value { get; }

    public override void Accept(
        IAstVisitor visitor
    )
    {
        visitor.Visit(this);
    }
}