namespace Ast.Expressions;

public sealed class IdentifierExpression : Expression
{
    public IdentifierExpression(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}