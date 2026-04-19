using Ast.Declarations;

namespace Ast.Expressions;

public sealed class CallExpression : Expression
{
    private BuiltinInfo? _builtin;
    private FunctionDeclaration? _function;

    public CallExpression(string name, List<Expression> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public string Name { get; }

    public List<Expression> Arguments { get; }

    public BuiltinInfo? Builtin
    {
        get => _builtin;
        set => _builtin = value;
    }

    public FunctionDeclaration? Function
    {
        get => _function;
        set => _function = value;
    }

    public bool IsBuiltin => _builtin is not null;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}