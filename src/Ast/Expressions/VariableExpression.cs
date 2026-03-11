namespace Ast.Expressions;

public sealed class VariableExpression : Expression
{
    public VariableExpression(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}