namespace Ast.Expressions;

public sealed class InputExpression : Expression
{
    public InputExpression(string variableName)
    {
        VariableName = variableName;
    }

    public string VariableName { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}

