namespace Ast.Declarations;

public sealed class FunctionDeclaration : Declaration
{
    public FunctionDeclaration(DataType returnType, string name, List<ParameterDeclaration> parameters, List<AstNode> body)
    {
        ReturnType = returnType;
        Name = name;
        Parameters = parameters;
        Body = body;
    }

    public DataType ReturnType { get; }

    public string Name { get; }

    public List<ParameterDeclaration> Parameters { get; }

    public List<AstNode> Body { get; }

    public bool IsProcedure => ReturnType == DataType.Void;

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}