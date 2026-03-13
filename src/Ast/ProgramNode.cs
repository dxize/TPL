using Ast.Declarations;

namespace Ast;

public sealed class ProgramNode : AstNode
{
    public ProgramNode(FunctionDeclaration mainFunction)
    {
        MainFunction = mainFunction;
    }

    public FunctionDeclaration MainFunction { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}