namespace Ast.Expressions;

public sealed class AssignmentExpression : Expression
{
    public AssignmentExpression(string name, Expression value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public Expression Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}