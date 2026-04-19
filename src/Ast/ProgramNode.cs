using Ast.Declarations;

namespace Ast;

public sealed class ProgramNode : AstNode
{
    public ProgramNode(List<Declaration> globalDeclarations, List<FunctionDeclaration> userFunctions, FunctionDeclaration mainFunction)
    {
        GlobalDeclarations = globalDeclarations;
        UserFunctions = userFunctions;
        MainFunction = mainFunction;
    }

    public List<Declaration> GlobalDeclarations { get; }

    public List<FunctionDeclaration> UserFunctions { get; }

    public FunctionDeclaration MainFunction { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}