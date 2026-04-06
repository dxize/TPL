namespace Ast.Expressions;

public sealed class BinaryExpression : Expression
{
    public BinaryExpression(Expression left, OperatorKind operatorKind, Expression right)
    {
        Left = left;
        OperatorKind = operatorKind;
        Right = right;
    }

    public Expression Left { get; }

    public OperatorKind OperatorKind { get; }

    public Expression Right { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}