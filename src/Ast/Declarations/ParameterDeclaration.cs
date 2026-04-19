namespace Ast.Declarations;

public sealed class ParameterDeclaration
{
    public ParameterDeclaration(DataType type, string name)
    {
        Type = type;
        Name = name;
    }

    public DataType Type { get; }

    public string Name { get; }
}