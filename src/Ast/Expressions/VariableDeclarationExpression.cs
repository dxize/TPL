using Ast.Declarations;

namespace Ast.Expressions;

public sealed class VariableDeclarationExpression : Declaration
{
    public VariableDeclarationExpression(DataType type, string name, Expression? initializer)
    {
        Type = type;
        Name = name;
        Initializer = initializer;
    }

    public DataType Type { get; }

    public string Name { get; }

    public Expression? Initializer { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}