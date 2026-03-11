using Ast.Declarations;

namespace Ast.Expressions;

public sealed class VariableScopeExpression : Expression
{
    private readonly List<VariableDeclaration> _variables;

    public VariableScopeExpression(List<VariableDeclaration> variables, Expression expression)
    {
        _variables = variables;
        Expression = expression;
    }

    public IReadOnlyList<VariableDeclaration> Variables => _variables;

    public Expression Expression { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}

