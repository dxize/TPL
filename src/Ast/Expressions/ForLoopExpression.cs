namespace Ast.Expressions;

public sealed class ForLoopExpression : Expression
{
    public ForLoopExpression(
        string iteratorName,
        Expression startValue,
        Expression endCondition,
        Expression? stepValue,
        List<AstNode> body
    )
    {
        IteratorName = iteratorName;
        StartValue = startValue;
        EndCondition = endCondition;
        StepValue = stepValue;
        Body = body;
    }

    public string IteratorName { get; }

    public Expression StartValue { get; }

    public Expression EndCondition { get; }

    public Expression? StepValue { get; }

    public List<AstNode> Body { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}