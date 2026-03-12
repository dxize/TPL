using Ast.Expressions;

namespace Ast.Declarations;

public sealed class ConstantDeclaration : Declaration
{
    public ConstantDeclaration(DataType type, string name, Expression value)
    {
        Type = type;
        Name = name;
        Value = value;
    }

    public DataType Type { get; }

    public string Name { get; }

    public Expression Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}