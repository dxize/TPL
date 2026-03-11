namespace Ast.Expressions;

public sealed class PrintExpression : Expression
{
    public PrintExpression(List<Expression> arguments)
    {
        Arguments = arguments;
    }

    public List<Expression> Arguments { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}

