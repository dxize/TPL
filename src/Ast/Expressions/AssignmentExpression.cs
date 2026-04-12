namespace Ast.Expressions;

public sealed class AssignmentExpression : Expression
{
    private SymbolInfo? _symbol;

    public AssignmentExpression(string name, Expression value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public Expression Value { get; }

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