using Ast.Declarations;

namespace Ast.Expressions;

public sealed class ProcedureCallStatement : AstNode
{
    private FunctionDeclaration? _procedure;

    public ProcedureCallStatement(string name, List<Expression> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public string Name { get; }

    public List<Expression> Arguments { get; }

    public FunctionDeclaration Procedure
    {
        get => _procedure!;
        set => _procedure = value;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}