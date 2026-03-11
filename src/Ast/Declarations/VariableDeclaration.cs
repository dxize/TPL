using Ast.Expressions;

namespace Ast.Declarations;

public sealed class VariableDeclaration : Declaration
{
    public VariableDeclaration(string name, Expression? value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public Expression? Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}