using Ast.Declarations;

namespace Ast;

public sealed class ProgramNode : AstNode
{
    public ProgramNode(List<Declaration> globalDeclarations, FunctionDeclaration mainFunction)
    {
        GlobalDeclarations = globalDeclarations;
        MainFunction = mainFunction;
    }

    public List<Declaration> GlobalDeclarations { get; }

    public FunctionDeclaration MainFunction { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}