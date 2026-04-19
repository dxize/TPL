namespace Ast.Expressions;

public sealed class IfStatement : AstNode
{
    public IfStatement(Expression condition, List<AstNode> thenBody, List<AstNode>? elseBody)
    {
        Condition = condition;
        ThenBody = thenBody;
        ElseBody = elseBody;
    }

    public Expression Condition { get; }

    public List<AstNode> ThenBody { get; }

    public List<AstNode>? ElseBody { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}