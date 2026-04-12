namespace Ast.Expressions;

public sealed class IdentifierExpression : Expression
{
    private SymbolInfo? _symbol;

    public IdentifierExpression(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public SymbolInfo Symbol
    {
        get => _symbol!;
        set => _symbol = value;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}