namespace Ast.Expressions;

public sealed class CallExpression : Expression
{
    private BuiltinInfo? _builtin;

    public CallExpression(string name, List<Expression> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public string Name { get; }

    public List<Expression> Arguments { get; }

    public BuiltinInfo Builtin
    {
        get => _builtin!;
        set => _builtin = value;
    }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}