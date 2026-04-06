namespace Ast.Expressions;

public sealed class UnaryExpression : Expression
{
    public UnaryExpression(OperatorKind operatorKind, Expression operand)
    {
        OperatorKind = operatorKind;
        Operand = operand;
    }

    public OperatorKind OperatorKind { get; }

    public Expression Operand { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}