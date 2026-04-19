namespace Ast.Expressions;

public sealed class ReturnExpression : Expression
{
    public ReturnExpression(Expression? value)
    {
        Value = value;
    }

    public Expression? Value { get; }

    public bool HasValue => Value is not null;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}