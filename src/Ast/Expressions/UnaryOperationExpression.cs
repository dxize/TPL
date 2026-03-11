namespace Ast.Expressions;

public sealed class UnaryOperationExpression : Expression
{
    public UnaryOperationExpression(UnaryOperation operation, Expression operand)
    {
        Operation = operation;
        Operand = operand;
    }

    public UnaryOperation Operation { get; }

    public Expression Operand { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}