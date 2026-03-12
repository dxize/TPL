namespace Ast.Expressions;

public sealed class LiteralExpression : Expression
{
    public LiteralExpression(DataType type, object value)
    {
        Type = type;
        Value = value;
    }

    public DataType Type { get; }

    public object Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}