namespace Ast.Expressions;

public sealed class InputExpression : Expression
{
    private SymbolInfo? _symbol;

    public InputExpression(string variableName)
    {
        VariableName = variableName;
    }

    public string VariableName { get; }

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