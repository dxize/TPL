namespace Ast.Declarations;

public sealed class FunctionDeclaration : Declaration
{
    public FunctionDeclaration(DataType? returnType, string name, List<Parameter> parameters, List<AstNode> body)
    {
        ReturnType = returnType;
        Name = name;
        Parameters = parameters;
        Body = body;
    }

    public DataType? ReturnType { get; }

    public string Name { get; }

    public List<Parameter> Parameters { get; }

    public List<AstNode> Body { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}