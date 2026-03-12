namespace Ast.Declarations;

public sealed class Parameter
{
    public Parameter(DataType type, string name)
    {
        Type = type;
        Name = name;
    }

    public DataType Type { get; }
    public string Name { get; }
}
