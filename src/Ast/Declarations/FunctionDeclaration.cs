namespace Ast.Declarations;

public sealed class FunctionDeclaration : Declaration
{
    public FunctionDeclaration(string name, List<string> parameters, List<AstNode> body)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
    }

    public string Name { get; }

    public List<string> Parameters { get; }

    public List<AstNode> Body { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}