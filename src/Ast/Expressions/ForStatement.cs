namespace Ast.Expressions;

public sealed class ForStatement : AstNode
{
    public ForStatement(string variableName, Expression start, Expression end, List<AstNode> body, bool descending)
    {
        VariableName = variableName;
        Start = start;
        End = end;
        Body = body;
        Descending = descending;
    }

    public string VariableName { get; }

    public Expression Start { get; }

    public Expression End { get; }

    public List<AstNode> Body { get; }

    public bool Descending { get; }

    public SymbolInfo? VariableSymbol { get; set; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}