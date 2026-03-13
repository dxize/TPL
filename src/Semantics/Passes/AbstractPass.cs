using Ast;
using Ast.Declarations;
using Ast.Expressions;

namespace Semantics.Passes;

/// <summary>
/// Базовый проход по AST. По умолчанию просто обходит дочерние узлы.
/// </summary>
public abstract class AbstractPass : IAstVisitor
{
    public virtual void Visit(ProgramNode p)
    {
        p.MainFunction.Accept(this);
    }

    public virtual void Visit(FunctionDeclaration d)
    {
        foreach (AstNode node in d.Body)
        {
            node.Accept(this);
        }
    }

    public virtual void Visit(LiteralExpression e)
    {
    }

    public virtual void Visit(PrintExpression e)
    {
        foreach (LiteralExpression argument in e.Arguments)
        {
            argument.Accept(this);
        }
    }

    public virtual void Visit(ReturnExpression e)
    {
        e.Value.Accept(this);
    }
}