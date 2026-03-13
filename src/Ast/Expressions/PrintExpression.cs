namespace Ast.Expressions;

public sealed class PrintExpression : Expression
{
    public PrintExpression(List<LiteralExpression> arguments)
    {
        Arguments = arguments;
    }

    public List<LiteralExpression> Arguments { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}